using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRentWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddRenewalDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "VpsRenewalDate",
                table: "DeffInformation",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DomainRenewalDate",
                table: "DeffInformation",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "SslRenewalDate",
                table: "DeffInformation",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "MessageRenewalDate",
                table: "DeffInformation",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VpsRenewalDate",
                table: "DeffInformation");

            migrationBuilder.DropColumn(
                name: "DomainRenewalDate",
                table: "DeffInformation");

            migrationBuilder.DropColumn(
                name: "SslRenewalDate",
                table: "DeffInformation");

            migrationBuilder.DropColumn(
                name: "MessageRenewalDate",
                table: "DeffInformation");
        }
    }
}
