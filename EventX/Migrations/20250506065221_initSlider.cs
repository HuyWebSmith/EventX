using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventX.Migrations
{
    /// <inheritdoc />
    public partial class initSlider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImagePath",
                table: "Sliders",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Sliders",
                newName: "CreatedDate");

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Sliders",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "Sliders");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Sliders",
                newName: "ImagePath");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Sliders",
                newName: "CreatedAt");
        }
    }
}
