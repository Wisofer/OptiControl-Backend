using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OptiControl.Migrations
{
    /// <inheritdoc />
    public partial class RenameCedulaToPasaporteInClients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Cedula",
                table: "Clients",
                newName: "Pasaporte");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Pasaporte",
                table: "Clients",
                newName: "Cedula");
        }
    }
}
