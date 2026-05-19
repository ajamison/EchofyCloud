using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Echofy.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddManufacturerProductLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ManufacturerProductId",
                table: "Products",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ManufacturerProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ManufacturerId = table.Column<int>(type: "integer", nullable: false),
                    ManufacturerPartNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManufacturerProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManufacturerProducts_Manufacturers_ManufacturerId",
                        column: x => x.ManufacturerId,
                        principalTable: "Manufacturers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ManufacturerProductImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ManufacturerProductId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    AltText = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    IsMain = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManufacturerProductImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManufacturerProductImages_ManufacturerProducts_Manufacturer~",
                        column: x => x.ManufacturerProductId,
                        principalTable: "ManufacturerProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_ManufacturerProductId",
                table: "Products",
                column: "ManufacturerProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturerProductImages_ManufacturerProductId_DisplayOrder",
                table: "ManufacturerProductImages",
                columns: new[] { "ManufacturerProductId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturerProducts_ManufacturerId",
                table: "ManufacturerProducts",
                column: "ManufacturerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ManufacturerProducts_ManufacturerProductId",
                table: "Products",
                column: "ManufacturerProductId",
                principalTable: "ManufacturerProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_ManufacturerProducts_ManufacturerProductId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "ManufacturerProductImages");

            migrationBuilder.DropTable(
                name: "ManufacturerProducts");

            migrationBuilder.DropIndex(
                name: "IX_Products_ManufacturerProductId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ManufacturerProductId",
                table: "Products");
        }
    }
}
