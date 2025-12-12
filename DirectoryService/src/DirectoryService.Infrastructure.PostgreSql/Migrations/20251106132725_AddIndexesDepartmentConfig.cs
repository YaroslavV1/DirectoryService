using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesDepartmentConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_locations_full_address",
                table: "locations");

            migrationBuilder.DropIndex(
                name: "ux_locations_name",
                table: "locations");

            migrationBuilder.DropIndex(
                name: "IX_departments_parent_id",
                table: "departments");

            migrationBuilder.CreateIndex(
                name: "ux_locations_full_address",
                table: "locations",
                columns: new[] { "city", "street", "house", "postal_code" },
                unique: true,
                filter: "\"is_active\" IS TRUE");

            migrationBuilder.CreateIndex(
                name: "ux_locations_name",
                table: "locations",
                column: "name",
                unique: true,
                filter: "\"is_active\" IS TRUE");

            migrationBuilder.CreateIndex(
                name: "ux_departments_parent_identifier",
                table: "departments",
                columns: new[] { "parent_id", "identifier" },
                unique: true,
                filter: "\"parent_id\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_departments_root_identifier",
                table: "departments",
                column: "identifier",
                unique: true,
                filter: "\"parent_id\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_locations_full_address",
                table: "locations");

            migrationBuilder.DropIndex(
                name: "ux_locations_name",
                table: "locations");

            migrationBuilder.DropIndex(
                name: "ux_departments_parent_identifier",
                table: "departments");

            migrationBuilder.DropIndex(
                name: "ux_departments_root_identifier",
                table: "departments");

            migrationBuilder.CreateIndex(
                name: "ux_locations_full_address",
                table: "locations",
                columns: new[] { "city", "street", "house", "postal_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_locations_name",
                table: "locations",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_departments_parent_id",
                table: "departments",
                column: "parent_id");
        }
    }
}
