using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRentWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddDriver : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          

            migrationBuilder.CreateTable(
                name: "DriverOnboardingSteps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StepName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StepOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverOnboardingSteps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeOnboardingProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    StepId = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeOnboardingProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeOnboardingProgresses_DriverOnboardingSteps_StepId",
                        column: x => x.StepId,
                        principalTable: "DriverOnboardingSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeOnboardingProgresses_EmployeeInfo_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "EmployeeInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeOnboardingProgresses_EmployeeId",
                table: "EmployeeOnboardingProgresses",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeOnboardingProgresses_StepId",
                table: "EmployeeOnboardingProgresses",
                column: "StepId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeOnboardingProgresses");

            migrationBuilder.DropTable(
                name: "DriverOnboardingSteps");

            migrationBuilder.DropColumn(
                name: "StampImagePath",
                table: "EmployeeInfo");

            migrationBuilder.DropColumn(
                name: "Domain",
                table: "DeffInformation");

            migrationBuilder.DropColumn(
                name: "DomainPassword",
                table: "DeffInformation");

            migrationBuilder.DropColumn(
                name: "DomainRenewalDate",
                table: "DeffInformation");

            migrationBuilder.DropColumn(
                name: "DomainUsername",
                table: "DeffInformation");

            migrationBuilder.DropColumn(
                name: "MessagePassword",
                table: "DeffInformation");

            migrationBuilder.DropColumn(
                name: "MessageRenewalDate",
                table: "DeffInformation");

            migrationBuilder.DropColumn(
                name: "MessageUrl",
                table: "DeffInformation");

            migrationBuilder.DropColumn(
                name: "MessageUsername",
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
                name: "SslPassword",
                table: "DeffInformation");

            migrationBuilder.DropColumn(
                name: "SslRenewalDate",
                table: "DeffInformation");

            migrationBuilder.DropColumn(
                name: "SslUrl",
                table: "DeffInformation");

            migrationBuilder.DropColumn(
                name: "SslUsername",
                table: "DeffInformation");

            migrationBuilder.DropColumn(
                name: "VpsRenewalDate",
                table: "DeffInformation");
        }
    }
}
