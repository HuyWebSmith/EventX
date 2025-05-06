using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventX.Migrations
{
    /// <inheritdoc />
    public partial class addSlider2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Slides",
                table: "Slides");

            migrationBuilder.RenameTable(
                name: "Slides",
                newName: "Sliders");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sliders",
                table: "Sliders",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Sliders",
                table: "Sliders");

            migrationBuilder.RenameTable(
                name: "Sliders",
                newName: "Slides");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Slides",
                table: "Slides",
                column: "Id");
        }
    }
}
