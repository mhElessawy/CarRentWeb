using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRentWeb.Migrations
{
    /// <inheritdoc />
    public partial class PeriodTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PeriodicTaskInstance_PeriodicTaskDef",
                table: "PeriodicTaskInstance");

            migrationBuilder.DropForeignKey(
                name: "FK_PeriodicTaskInstanceStep_Instance",
                table: "PeriodicTaskInstanceStep");

            migrationBuilder.DropForeignKey(
                name: "FK_PeriodicTaskInstanceStep_Step",
                table: "PeriodicTaskInstanceStep");

            migrationBuilder.DropForeignKey(
                name: "FK_PeriodicTaskStep_PeriodicTaskDef",
                table: "PeriodicTaskStep");

            migrationBuilder.AlterColumn<string>(
                name: "StepName",
                table: "PeriodicTaskStep",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "PeriodicTaskStep",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Authority",
                table: "PeriodicTaskStep",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsCompleted",
                table: "PeriodicTaskInstanceStep",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsCompleted",
                table: "PeriodicTaskInstance",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "TargetDate",
                table: "PeriodicTaskInstance",
                type: "date",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TaskName",
                table: "PeriodicTaskDef",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "SourceType",
                table: "PeriodicTaskDef",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "PeriodicTaskDef",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "DateFieldName",
                table: "PeriodicTaskDef",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "AlertDaysBefore",
                table: "PeriodicTaskDef",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 30);

            migrationBuilder.AddForeignKey(
                name: "FK_PeriodicTaskInstance_PeriodicTaskDef_TaskDefId",
                table: "PeriodicTaskInstance",
                column: "TaskDefId",
                principalTable: "PeriodicTaskDef",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PeriodicTaskInstanceStep_PeriodicTaskInstance_InstanceId",
                table: "PeriodicTaskInstanceStep",
                column: "InstanceId",
                principalTable: "PeriodicTaskInstance",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PeriodicTaskInstanceStep_PeriodicTaskStep_StepId",
                table: "PeriodicTaskInstanceStep",
                column: "StepId",
                principalTable: "PeriodicTaskStep",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PeriodicTaskStep_PeriodicTaskDef_TaskDefId",
                table: "PeriodicTaskStep",
                column: "TaskDefId",
                principalTable: "PeriodicTaskDef",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PeriodicTaskInstance_PeriodicTaskDef_TaskDefId",
                table: "PeriodicTaskInstance");

            migrationBuilder.DropForeignKey(
                name: "FK_PeriodicTaskInstanceStep_PeriodicTaskInstance_InstanceId",
                table: "PeriodicTaskInstanceStep");

            migrationBuilder.DropForeignKey(
                name: "FK_PeriodicTaskInstanceStep_PeriodicTaskStep_StepId",
                table: "PeriodicTaskInstanceStep");

            migrationBuilder.DropForeignKey(
                name: "FK_PeriodicTaskStep_PeriodicTaskDef_TaskDefId",
                table: "PeriodicTaskStep");

            migrationBuilder.DropColumn(
                name: "TargetDate",
                table: "PeriodicTaskInstance");

            migrationBuilder.AlterColumn<string>(
                name: "StepName",
                table: "PeriodicTaskStep",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "PeriodicTaskStep",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Authority",
                table: "PeriodicTaskStep",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsCompleted",
                table: "PeriodicTaskInstanceStep",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsCompleted",
                table: "PeriodicTaskInstance",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "TaskName",
                table: "PeriodicTaskDef",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "SourceType",
                table: "PeriodicTaskDef",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "PeriodicTaskDef",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "DateFieldName",
                table: "PeriodicTaskDef",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "AlertDaysBefore",
                table: "PeriodicTaskDef",
                type: "int",
                nullable: false,
                defaultValue: 30,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_PeriodicTaskInstance_PeriodicTaskDef",
                table: "PeriodicTaskInstance",
                column: "TaskDefId",
                principalTable: "PeriodicTaskDef",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PeriodicTaskInstanceStep_Instance",
                table: "PeriodicTaskInstanceStep",
                column: "InstanceId",
                principalTable: "PeriodicTaskInstance",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PeriodicTaskInstanceStep_Step",
                table: "PeriodicTaskInstanceStep",
                column: "StepId",
                principalTable: "PeriodicTaskStep",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PeriodicTaskStep_PeriodicTaskDef",
                table: "PeriodicTaskStep",
                column: "TaskDefId",
                principalTable: "PeriodicTaskDef",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
