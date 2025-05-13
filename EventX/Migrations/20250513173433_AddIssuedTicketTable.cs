using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventX.Migrations
{
    /// <inheritdoc />
    public partial class AddIssuedTicketTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IssuedTickets",
                columns: table => new
                {
                    IssuedTicketID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderDetailID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssuedTickets", x => x.IssuedTicketID);
                    table.ForeignKey(
                        name: "FK_IssuedTickets_OrderDetail_OrderDetailID",
                        column: x => x.OrderDetailID,
                        principalTable: "OrderDetail",
                        principalColumn: "OrderDetailID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IssuedTickets_OrderDetailID",
                table: "IssuedTickets",
                column: "OrderDetailID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IssuedTickets");
        }
    }
}
