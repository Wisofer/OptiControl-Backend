using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OptiControl.Migrations
{
    /// <inheritdoc />
    public partial class AddTravelAndReturnDateToInvoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnDate",
                table: "Invoices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TravelDate",
                table: "Invoices",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReturnDate",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "TravelDate",
                table: "Invoices");
        }
    }
}
