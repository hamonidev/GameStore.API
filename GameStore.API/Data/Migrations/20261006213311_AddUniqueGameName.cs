using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameStore.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueGameName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Games_Name",
                table: "Games",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Games_Name",
                table: "Games");
        }
    }
}
