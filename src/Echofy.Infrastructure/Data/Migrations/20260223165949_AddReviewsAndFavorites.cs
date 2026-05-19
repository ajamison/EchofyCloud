using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echofy.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewsAndFavorites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Customers_CustomerId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_CustomerId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Reviews");

            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "Reviews",
                type: "character varying(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Reviews",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "FavoriteProducts",
                columns: table => new
                {
                    AppUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoriteProducts", x => new { x.AppUserId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_FavoriteProducts_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FavoriteProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_AppUserId_ProductId",
                table: "Reviews",
                columns: new[] { "AppUserId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteProducts_ProductId",
                table: "FavoriteProducts",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_AspNetUsers_AppUserId",
                table: "Reviews",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_AspNetUsers_AppUserId",
                table: "Reviews");

            migrationBuilder.DropTable(
                name: "FavoriteProducts");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_AppUserId_ProductId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Reviews");

            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "Reviews",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_CustomerId",
                table: "Reviews",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Customers_CustomerId",
                table: "Reviews",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
