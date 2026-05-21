using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRentWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddMoiFieldsToCompanyInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MoiUnifiedNo",
                table: "CompanyInfo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "MoiExpiryDate",
                table: "CompanyInfo",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MoiAllowedCars",
                table: "CompanyInfo",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "MoiUnifiedNo",   table: "CompanyInfo");
            migrationBuilder.DropColumn(name: "MoiExpiryDate",  table: "CompanyInfo");
            migrationBuilder.DropColumn(name: "MoiAllowedCars", table: "CompanyInfo");
        }
    }
}
