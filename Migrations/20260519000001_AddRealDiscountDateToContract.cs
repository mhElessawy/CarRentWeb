using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRentWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddRealDiscountDateToContract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "RealDiscountDate",
                table: "Contract",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RealDiscountDate",
                table: "Contract");
        }
    }
}
