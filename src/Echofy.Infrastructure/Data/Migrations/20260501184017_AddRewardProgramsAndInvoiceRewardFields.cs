using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Echofy.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRewardProgramsAndInvoiceRewardFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "TotalAmount",
                table: "Invoices",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<decimal>(
                name: "RewardGiftCardAmount",
                table: "Invoices",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "RewardGiftCardCode",
                table: "Invoices",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RewardPointsAwarded",
                table: "Invoices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "AllowCompanyRewardOverride",
                table: "Clients",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "RewardPrograms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RewardPrograms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RewardPrograms_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RewardPrograms_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RewardTiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RewardProgramId = table.Column<int>(type: "integer", nullable: false),
                    Label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MinInvoiceAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PointsForReferrer = table.Column<int>(type: "integer", nullable: false),
                    GiftCardAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RewardTiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RewardTiers_RewardPrograms_RewardProgramId",
                        column: x => x.RewardProgramId,
                        principalTable: "RewardPrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RewardPrograms_ClientId",
                table: "RewardPrograms",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_RewardPrograms_CompanyId",
                table: "RewardPrograms",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_RewardTiers_RewardProgramId",
                table: "RewardTiers",
                column: "RewardProgramId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RewardTiers");

            migrationBuilder.DropTable(
                name: "RewardPrograms");

            migrationBuilder.DropColumn(
                name: "RewardGiftCardAmount",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "RewardGiftCardCode",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "RewardPointsAwarded",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "AllowCompanyRewardOverride",
                table: "Clients");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalAmount",
                table: "Invoices",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");
        }
    }
}
