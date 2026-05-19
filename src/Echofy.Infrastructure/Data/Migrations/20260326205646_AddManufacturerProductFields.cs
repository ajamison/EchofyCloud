using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echofy.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddManufacturerProductFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ManufacturerProducts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Msrp",
                table: "ManufacturerProducts",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "ManufacturerProducts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sku",
                table: "ManufacturerProducts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitOfMeasureId",
                table: "ManufacturerProducts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturerProducts_UnitOfMeasureId",
                table: "ManufacturerProducts",
                column: "UnitOfMeasureId");

            migrationBuilder.AddForeignKey(
                name: "FK_ManufacturerProducts_UnitsOfMeasure_UnitOfMeasureId",
                table: "ManufacturerProducts",
                column: "UnitOfMeasureId",
                principalTable: "UnitsOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ManufacturerProducts_UnitsOfMeasure_UnitOfMeasureId",
                table: "ManufacturerProducts");

            migrationBuilder.DropIndex(
                name: "IX_ManufacturerProducts_UnitOfMeasureId",
                table: "ManufacturerProducts");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ManufacturerProducts");

            migrationBuilder.DropColumn(
                name: "Msrp",
                table: "ManufacturerProducts");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "ManufacturerProducts");

            migrationBuilder.DropColumn(
                name: "Sku",
                table: "ManufacturerProducts");

            migrationBuilder.DropColumn(
                name: "UnitOfMeasureId",
                table: "ManufacturerProducts");
        }
    }
}
