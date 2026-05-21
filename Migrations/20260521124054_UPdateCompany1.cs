using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRentWeb.Migrations
{
    /// <inheritdoc />
    public partial class UPdateCompany1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MoiAllowedCars",
                table: "CompanyInfo",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "MoiExpiryDate",
                table: "CompanyInfo",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MoiUnifiedNo",
                table: "CompanyInfo",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MoiAllowedCars",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "MoiExpiryDate",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "MoiUnifiedNo",
                table: "CompanyInfo");
        }
    }
}
