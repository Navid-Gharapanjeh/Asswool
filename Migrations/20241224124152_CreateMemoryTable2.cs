using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asswool.Migrations
{
    /// <inheritdoc />
    public partial class CreateMemoryTable2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Memories",
                columns: new[] { "Id", "Key", "Value" },
                values: new object[] { 1, "favorite_color", "blue" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Memories",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
