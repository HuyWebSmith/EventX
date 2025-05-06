using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventX.Migrations
{
    /// <inheritdoc />
    public partial class removeOrganizer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Event_AspNetUsers_OrganizerId",
                table: "Event");

            migrationBuilder.DropIndex(
                name: "IX_Event_OrganizerId",
                table: "Event");

            migrationBuilder.AlterColumn<string>(
                name: "OrganizerId",
                table: "Event",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "OrganizerId",
                table: "Event",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Event_OrganizerId",
                table: "Event",
                column: "OrganizerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Event_AspNetUsers_OrganizerId",
                table: "Event",
                column: "OrganizerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
