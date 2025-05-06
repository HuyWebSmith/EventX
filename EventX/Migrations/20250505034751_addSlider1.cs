using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventX.Migrations
{
    /// <inheritdoc />
    public partial class addSlider1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Slides",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Slides",
                newName: "Link");

            migrationBuilder.RenameColumn(
                name: "Image",
                table: "Slides",
                newName: "ImagePath");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Slides",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Slides",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Slides");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Slides");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Slides",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Link",
                table: "Slides",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "ImagePath",
                table: "Slides",
                newName: "Image");
        }
    }
}
