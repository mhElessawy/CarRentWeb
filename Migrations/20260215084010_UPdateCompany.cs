using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRentWeb.Migrations
{
    /// <inheritdoc />
    public partial class UPdateCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActivitiesCount",
                table: "CompanyInfo",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActivityCode",
                table: "CompanyInfo",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressBuildingName",
                table: "CompanyInfo",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressStreet",
                table: "CompanyInfo",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressUnitNo",
                table: "CompanyInfo",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "AuthorizationDate",
                table: "CompanyInfo",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthorizedPersonCivilId",
                table: "CompanyInfo",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthorizedPersonName",
                table: "CompanyInfo",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Capital",
                table: "CompanyInfo",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CivilEconomicActivity",
                table: "CompanyInfo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "CivilInfoDate",
                table: "CompanyInfo",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CivilInfoRef",
                table: "CompanyInfo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CivilLatinName",
                table: "CompanyInfo",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CivilLicenseAuthority",
                table: "CompanyInfo",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CivilLicenseHolder",
                table: "CompanyInfo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CivilLicenseName",
                table: "CompanyInfo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CivilSignboardName",
                table: "CompanyInfo",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyStatus",
                table: "CompanyInfo",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FiscalYearEndMonth",
                table: "CompanyInfo",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FiscalYearStartMonth",
                table: "CompanyInfo",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GovContractsCount",
                table: "CompanyInfo",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "InsuranceCertDate",
                table: "CompanyInfo",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "InsuranceCertValidDate",
                table: "CompanyInfo",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsuranceRef",
                table: "CompanyInfo",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsuranceRegNo",
                table: "CompanyInfo",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsuranceSector",
                table: "CompanyInfo",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegalEntity",
                table: "CompanyInfo",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LicenseType",
                table: "CompanyInfo",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MainLicenseStatus",
                table: "CompanyInfo",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagerPermissions",
                table: "CompanyInfo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ManagersCount",
                table: "CompanyInfo",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ManpowerCertExpiryDate",
                table: "CompanyInfo",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ManpowerCertIssueDate",
                table: "CompanyInfo",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManpowerFileClassification",
                table: "CompanyInfo",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManpowerFileNo",
                table: "CompanyInfo",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManpowerFileType",
                table: "CompanyInfo",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManpowerWorkAdmin",
                table: "CompanyInfo",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PartnersCount",
                table: "CompanyInfo",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "RegistrationDate",
                table: "CompanyInfo",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RentContractNo",
                table: "CompanyInfo",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TradeLicenseNo",
                table: "CompanyInfo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TradeName",
                table: "CompanyInfo",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnifiedNo",
                table: "CompanyInfo",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CompanyApproval",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompId = table.Column<int>(type: "int", nullable: false),
                    AuthorityName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ApprovalNo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ApprovalDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyApproval", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyApproval_CompanyInfo",
                        column: x => x.CompId,
                        principalTable: "CompanyInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyPartner",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompId = table.Column<int>(type: "int", nullable: false),
                    PartnerName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Role = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SharePercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyPartner", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyPartner_CompanyInfo",
                        column: x => x.CompId,
                        principalTable: "CompanyInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyApproval_CompId",
                table: "CompanyApproval",
                column: "CompId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyPartner_CompId",
                table: "CompanyPartner",
                column: "CompId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyApproval");

            migrationBuilder.DropTable(
                name: "CompanyPartner");

            migrationBuilder.DropColumn(
                name: "ActivitiesCount",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "ActivityCode",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "AddressBuildingName",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "AddressStreet",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "AddressUnitNo",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "AuthorizationDate",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "AuthorizedPersonCivilId",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "AuthorizedPersonName",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "Capital",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "CivilEconomicActivity",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "CivilInfoDate",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "CivilInfoRef",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "CivilLatinName",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "CivilLicenseAuthority",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "CivilLicenseHolder",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "CivilLicenseName",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "CivilSignboardName",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "CompanyStatus",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "FiscalYearEndMonth",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "FiscalYearStartMonth",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "GovContractsCount",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "InsuranceCertDate",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "InsuranceCertValidDate",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "InsuranceRef",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "InsuranceRegNo",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "InsuranceSector",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "LegalEntity",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "LicenseType",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "MainLicenseStatus",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "ManagerPermissions",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "ManagersCount",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "ManpowerCertExpiryDate",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "ManpowerCertIssueDate",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "ManpowerFileClassification",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "ManpowerFileNo",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "ManpowerFileType",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "ManpowerWorkAdmin",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "PartnersCount",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "RegistrationDate",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "RentContractNo",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "TradeLicenseNo",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "TradeName",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "UnifiedNo",
                table: "CompanyInfo");
        }
    }
}
