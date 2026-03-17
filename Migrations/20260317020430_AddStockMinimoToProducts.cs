using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OptiControl.Migrations
{
    /// <inheritdoc />
    public partial class AddStockMinimoToProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StockMinimo",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockMinimo",
                table: "Products");
        }
    }
}
