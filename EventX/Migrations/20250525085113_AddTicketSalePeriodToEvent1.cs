using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventX.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketSalePeriodToEvent1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TicketSaleEnd",
                table: "Event");

            migrationBuilder.DropColumn(
                name: "TicketSaleStart",
                table: "Event");

            migrationBuilder.AddColumn<DateTime>(
                name: "TicketSaleEnd",
                table: "Tickets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TicketSaleStart",
                table: "Tickets",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TicketSaleEnd",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "TicketSaleStart",
                table: "Tickets");

            migrationBuilder.AddColumn<DateTime>(
                name: "TicketSaleEnd",
                table: "Event",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TicketSaleStart",
                table: "Event",
                type: "datetime2",
                nullable: true);
        }
    }
}
