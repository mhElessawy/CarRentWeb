using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRentWeb.Migrations
{
    /// <inheritdoc />
    public partial class UPdateContractDaily : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "Contract",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DiscountDate",
                table: "Contract",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RentalType",
                table: "Contract",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "Contract");

            migrationBuilder.DropColumn(
                name: "DiscountDate",
                table: "Contract");

            migrationBuilder.DropColumn(
                name: "RentalType",
                table: "Contract");
        }
    }
}
