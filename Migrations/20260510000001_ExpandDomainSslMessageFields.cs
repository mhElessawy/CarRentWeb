using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRentWeb.Migrations
{
    /// <inheritdoc />
    public partial class ExpandDomainSslMessageFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DomainRenewalDate",
                table: "DeffInformation",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DomainUsername",
                table: "DeffInformation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DomainPassword",
                table: "DeffInformation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SslUrl",
                table: "DeffInformation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SslUsername",
                table: "DeffInformation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SslPassword",
                table: "DeffInformation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MessageUrl",
                table: "DeffInformation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MessageRenewalDate",
                table: "DeffInformation",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MessageUsername",
                table: "DeffInformation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MessagePassword",
                table: "DeffInformation",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "DomainRenewalDate", table: "DeffInformation");
            migrationBuilder.DropColumn(name: "DomainUsername", table: "DeffInformation");
            migrationBuilder.DropColumn(name: "DomainPassword", table: "DeffInformation");
            migrationBuilder.DropColumn(name: "SslUrl", table: "DeffInformation");
            migrationBuilder.DropColumn(name: "SslUsername", table: "DeffInformation");
            migrationBuilder.DropColumn(name: "SslPassword", table: "DeffInformation");
            migrationBuilder.DropColumn(name: "MessageUrl", table: "DeffInformation");
            migrationBuilder.DropColumn(name: "MessageRenewalDate", table: "DeffInformation");
            migrationBuilder.DropColumn(name: "MessageUsername", table: "DeffInformation");
            migrationBuilder.DropColumn(name: "MessagePassword", table: "DeffInformation");
        }
    }
}