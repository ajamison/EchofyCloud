using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echofy.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProductPrimaryShortId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_ShortId",
                table: "Products");

            migrationBuilder.AlterColumn<string>(
                name: "ShortId",
                table: "Products",
                type: "character varying(12)",
                maxLength: 12,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(12)",
                oldMaxLength: 12);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ShortId",
                table: "Products",
                type: "character varying(12)",
                maxLength: 12,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(12)",
                oldMaxLength: 12,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ShortId",
                table: "Products",
                column: "ShortId",
                unique: true);
        }
    }
}
