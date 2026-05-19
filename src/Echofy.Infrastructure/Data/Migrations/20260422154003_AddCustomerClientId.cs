using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echofy.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerClientId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "Customers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_ClientId",
                table: "Customers",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Clients_ClientId",
                table: "Customers",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Clients_ClientId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_ClientId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Customers");
        }
    }
}
