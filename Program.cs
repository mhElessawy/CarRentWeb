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
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<WhatsAppService>();


builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

var app = builder.Build();

// Apply pending migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CarRentWebContext>();
    db.Database.Migrate();

    // Add new DeffInformation columns if they don't exist
    db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DeffInformation') AND name = 'VpsRenewalDate')
            ALTER TABLE DeffInformation ADD VpsRenewalDate datetime2 NULL;
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DeffInformation') AND name = 'SiteUrl')
            ALTER TABLE DeffInformation ADD SiteUrl nvarchar(max) NULL;
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DeffInformation') AND name = 'SiteUsername')
            ALTER TABLE DeffInformation ADD SiteUsername nvarchar(max) NULL;
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DeffInformation') AND name = 'SitePassword')
            ALTER TABLE DeffInformation ADD SitePassword nvarchar(max) NULL;
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DeffInformation') AND name = 'Domain')
            ALTER TABLE DeffInformation ADD Domain nvarchar(max) NULL;
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DeffInformation') AND name = 'DomainRenewalDate')
            ALTER TABLE DeffInformation ADD DomainRenewalDate datetime2 NULL;
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DeffInformation') AND name = 'DomainUsername')
            ALTER TABLE DeffInformation ADD DomainUsername nvarchar(max) NULL;
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DeffInformation') AND name = 'DomainPassword')
            ALTER TABLE DeffInformation ADD DomainPassword nvarchar(max) NULL;
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DeffInformation') AND name = 'SslUrl')
            ALTER TABLE DeffInformation ADD SslUrl nvarchar(max) NULL;
        IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DeffInformation') AND name = 'SslExpiry')
            EXEC sp_rename 'DeffInformation.SslExpiry', 'SslRenewalDate', 'COLUMN';
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DeffInformation') AND name = 'SslRenewalDate')
            ALTER TABLE DeffInformation ADD SslRenewalDate datetime2 NULL;
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DeffInformation') AND name = 'SslUsername')
            ALTER TABLE DeffInformation ADD SslUsername nvarchar(max) NULL;
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DeffInformation') AND name = 'SslPassword')
            ALTER TABLE DeffInformation ADD SslPassword nvarchar(max) NULL;
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DeffInformation') AND name = 'MessageUrl')
            ALTER TABLE DeffInformation ADD MessageUrl nvarchar(max) NULL;
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DeffInformation') AND name = 'MessageRenewalDate')
            ALTER TABLE DeffInformation ADD MessageRenewalDate datetime2 NULL;
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DeffInformation') AND name = 'MessageUsername')
            ALTER TABLE DeffInformation ADD MessageUsername nvarchar(max) NULL;
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DeffInformation') AND name = 'MessagePassword')
            ALTER TABLE DeffInformation ADD MessagePassword nvarchar(max) NULL;
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