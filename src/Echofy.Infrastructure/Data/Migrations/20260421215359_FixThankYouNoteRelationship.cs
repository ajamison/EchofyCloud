using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echofy.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixThankYouNoteRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ThankYouNotes_Invoices_InvoiceId1",
                table: "ThankYouNotes");

            migrationBuilder.DropIndex(
                name: "IX_ThankYouNotes_InvoiceId1",
                table: "ThankYouNotes");

            migrationBuilder.DropColumn(
                name: "InvoiceId1",
                table: "ThankYouNotes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InvoiceId1",
                table: "ThankYouNotes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThankYouNotes_InvoiceId1",
                table: "ThankYouNotes",
                column: "InvoiceId1",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ThankYouNotes_Invoices_InvoiceId1",
                table: "ThankYouNotes",
                column: "InvoiceId1",
                principalTable: "Invoices",
                principalColumn: "Id");
        }
    }
}
