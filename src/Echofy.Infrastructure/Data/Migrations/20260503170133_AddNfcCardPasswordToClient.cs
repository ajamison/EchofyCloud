using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echofy.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNfcCardPasswordToClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NfcCardPassword",
                table: "Clients",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NfcCardPassword",
                table: "Clients");
        }
    }
}
