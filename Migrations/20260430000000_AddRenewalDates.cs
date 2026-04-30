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
            migrationBuilder.AddColumn<DateTime>(
                name: "VpsRenewalDate",
                table: "DeffInformation",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DomainRenewalDate",
                table: "DeffInformation",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SslRenewalDate",
                table: "DeffInformation",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MessageRenewalDate",
                table: "DeffInformation",
                type: "datetime2",
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
