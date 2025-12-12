using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesLocationConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
