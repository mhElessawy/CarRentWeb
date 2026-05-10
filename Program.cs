using CarRentWeb.Data;
using CarRentWeb.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<CarRentWebContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<CarRentWebContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ContractDocService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<WhatsAppService>();


builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

var app = builder.Build();

// Apply pending migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CarRentWebContext>();
    try { db.Database.Migrate(); } catch { }

    // Ensure columns added outside EF migration tooling exist in the DB
    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (
            SELECT 1 FROM sys.columns
            WHERE object_id = OBJECT_ID(N'EmployeeInfo') AND name = N'StampImagePath'
        )
        ALTER TABLE [EmployeeInfo] ADD [StampImagePath] nvarchar(max) NULL;
    ");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=PasswordDatums}/{action=login}/{id?}");
app.MapRazorPages();
app.Run();