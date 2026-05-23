using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRentWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddPeriodicTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PeriodicTaskDef",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DateFieldName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AlertDaysBefore = table.Column<int>(type: "int", nullable: false, defaultValue: 30),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodicTaskDef", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PeriodicTaskStep",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskDefId = table.Column<int>(type: "int", nullable: false),
                    StepOrder = table.Column<int>(type: "int", nullable: false),
                    StepName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Authority = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodicTaskStep", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeriodicTaskStep_PeriodicTaskDef",
                        column: x => x.TaskDefId,
                        principalTable: "PeriodicTaskDef",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PeriodicTaskInstance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskDefId = table.Column<int>(type: "int", nullable: false),
                    SourceId = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodicTaskInstance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeriodicTaskInstance_PeriodicTaskDef",
                        column: x => x.TaskDefId,
                        principalTable: "PeriodicTaskDef",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PeriodicTaskInstanceStep",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstanceId = table.Column<int>(type: "int", nullable: false),
                    StepId = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CompletedDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodicTaskInstanceStep", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeriodicTaskInstanceStep_Instance",
                        column: x => x.InstanceId,
                        principalTable: "PeriodicTaskInstance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PeriodicTaskInstanceStep_Step",
                        column: x => x.StepId,
                        principalTable: "PeriodicTaskStep",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicTaskStep_TaskDefId",
                table: "PeriodicTaskStep",
                column: "TaskDefId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicTaskInstance_TaskDefId",
                table: "PeriodicTaskInstance",
                column: "TaskDefId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicTaskInstanceStep_InstanceId",
                table: "PeriodicTaskInstanceStep",
                column: "InstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicTaskInstanceStep_StepId",
                table: "PeriodicTaskInstanceStep",
                column: "StepId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PeriodicTaskInstanceStep");
            migrationBuilder.DropTable(name: "PeriodicTaskInstance");
            migrationBuilder.DropTable(name: "PeriodicTaskStep");
            migrationBuilder.DropTable(name: "PeriodicTaskDef");
        }
    }
}
