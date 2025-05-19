using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventX.Migrations
{
    /// <inheritdoc />
    public partial class AddUserToIssuedTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "IssuedTickets",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_IssuedTickets_UserId",
                table: "IssuedTickets",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_IssuedTickets_AspNetUsers_UserId",
                table: "IssuedTickets",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IssuedTickets_AspNetUsers_UserId",
                table: "IssuedTickets");

            migrationBuilder.DropIndex(
                name: "IX_IssuedTickets_UserId",
                table: "IssuedTickets");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "IssuedTickets");
        }
    }
}
