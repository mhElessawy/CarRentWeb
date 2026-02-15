using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRentWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddRentalTypeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RentalType",
                table: "Contract",
                type: "int",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DiscountDate",
                table: "Contract",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "Contract",
                type: "money",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RentalType",
                table: "Contract");

            migrationBuilder.DropColumn(
                name: "DiscountDate",
                table: "Contract");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "Contract");
        }
    }
}
