using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRentWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddVpsDomainFieldsToDeffInformation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Domain",
                table: "DeffInformation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "DeffInformation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SitePassword",
                table: "DeffInformation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SiteUrl",
                table: "DeffInformation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SiteUsername",
                table: "DeffInformation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SslExpiry",
                table: "DeffInformation",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VpsRenewalDate",
                table: "DeffInformation",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Domain",
                table: "DeffInformation");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "DeffInformation");

            migrationBuilder.DropColumn(
                name: "SitePassword",
                table: "DeffInformation");

            migrationBuilder.DropColumn(
                name: "SiteUrl",
                table: "DeffInformation");

            migrationBuilder.DropColumn(
                name: "SiteUsername",
                table: "DeffInformation");

            migrationBuilder.DropColumn(
                name: "SslExpiry",
                table: "DeffInformation");

            migrationBuilder.DropColumn(
                name: "VpsRenewalDate",
                table: "DeffInformation");
        }
    }
}