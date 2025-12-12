using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixConfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_departments_departments_parent_id",
                table: "departments");

            migrationBuilder.DropPrimaryKey(
                name: "pk_department_positions",
                table: "department_positions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_department_locations",
                table: "department_locations");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "positions",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "locations",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "departments",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "departments",
                newName: "is_active");

            migrationBuilder.AlterColumn<int>(
                name: "depth",
                table: "departments",
                type: "integer",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AddColumn<int>(
                name: "children_count",
                table: "departments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "id",
                table: "department_positions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "id",
                table: "department_locations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_department_positions",
                table: "department_positions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_department_locations",
                table: "department_locations",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_department_positions_department_id",
                table: "department_positions",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_department_locations_department_id",
                table: "department_locations",
                column: "department_id");

            migrationBuilder.AddForeignKey(
                name: "FK_departments_departments_parent_id",
                table: "departments",
                column: "parent_id",
                principalTable: "departments",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_departments_departments_parent_id",
                table: "departments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_department_positions",
                table: "department_positions");

            migrationBuilder.DropIndex(
                name: "IX_department_positions_department_id",
                table: "department_positions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_department_locations",
                table: "department_locations");

            migrationBuilder.DropIndex(
                name: "IX_department_locations_department_id",
                table: "department_locations");

            migrationBuilder.DropColumn(
                name: "children_count",
                table: "departments");

            migrationBuilder.DropColumn(
                name: "id",
                table: "department_positions");

            migrationBuilder.DropColumn(
                name: "id",
                table: "department_locations");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "positions",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "locations",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "departments",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "departments",
                newName: "IsActive");

            migrationBuilder.AlterColumn<short>(
                name: "depth",
                table: "departments",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddPrimaryKey(
                name: "pk_department_positions",
                table: "department_positions",
                columns: new[] { "department_id", "position_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_department_locations",
                table: "department_locations",
                columns: new[] { "department_id", "location_id" });

            migrationBuilder.AddForeignKey(
                name: "FK_departments_departments_parent_id",
                table: "departments",
                column: "parent_id",
                principalTable: "departments",
                principalColumn: "Id");
        }
    }
}
