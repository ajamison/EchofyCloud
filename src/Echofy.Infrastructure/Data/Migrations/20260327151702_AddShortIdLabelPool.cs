using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echofy.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddShortIdLabelPool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductShortIds_Products_ProductId",
                table: "ProductShortIds");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "ProductShortIds",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedAt",
                table: "ProductShortIds",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductShortIds_Products_ProductId",
                table: "ProductShortIds",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductShortIds_Products_ProductId",
                table: "ProductShortIds");

            migrationBuilder.DropColumn(
                name: "AssignedAt",
                table: "ProductShortIds");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "ProductShortIds",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductShortIds_Products_ProductId",
                table: "ProductShortIds",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
