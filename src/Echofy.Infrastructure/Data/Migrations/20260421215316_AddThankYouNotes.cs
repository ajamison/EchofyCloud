using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Echofy.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddThankYouNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ThankYouNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InvoiceId = table.Column<int>(type: "integer", nullable: false),
                    ClientId = table.Column<int>(type: "integer", nullable: true),
                    CustomerEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CustomMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReferralIncluded = table.Column<bool>(type: "boolean", nullable: false),
                    ReferralCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ReferralUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InvoiceId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThankYouNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThankYouNotes_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ThankYouNotes_Invoices_InvoiceId1",
                        column: x => x.InvoiceId1,
                        principalTable: "Invoices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ThankYouNotes_InvoiceId",
                table: "ThankYouNotes",
                column: "InvoiceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThankYouNotes_InvoiceId1",
                table: "ThankYouNotes",
                column: "InvoiceId1",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ThankYouNotes");
        }
    }
}
