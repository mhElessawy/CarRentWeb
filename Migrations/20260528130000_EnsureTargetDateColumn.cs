using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRentWeb.Migrations
{
    /// <inheritdoc />
    public partial class EnsureTargetDateColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = 'PeriodicTaskInstance'
                      AND COLUMN_NAME = 'TargetDate'
                )
                BEGIN
                    ALTER TABLE PeriodicTaskInstance ADD TargetDate date NULL;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = 'PeriodicTaskInstance'
                      AND COLUMN_NAME = 'TargetDate'
                )
                BEGIN
                    ALTER TABLE PeriodicTaskInstance DROP COLUMN TargetDate;
                END
            ");
        }
    }
}