using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRentWeb.Migrations
{
    /// <inheritdoc />
    public partial class updatewEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BloodType",
                table: "EmployeeInfo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Education",
                table: "EmployeeInfo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaritalStatus",
                table: "EmployeeInfo",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Religion",
                table: "EmployeeInfo",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BloodType",
                table: "EmployeeInfo");

            migrationBuilder.DropColumn(
                name: "Education",
                table: "EmployeeInfo");

            migrationBuilder.DropColumn(
                name: "MaritalStatus",
                table: "EmployeeInfo");

            migrationBuilder.DropColumn(
                name: "Religion",
                table: "EmployeeInfo");
        }
    }
}
