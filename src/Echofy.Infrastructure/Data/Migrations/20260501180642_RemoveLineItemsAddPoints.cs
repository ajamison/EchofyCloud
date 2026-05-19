using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echofy.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLineItemsAddPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "ReferralRewards");

            migrationBuilder.DropColumn(
                name: "GiftCardCode",
                table: "ReferralRewards");

            migrationBuilder.DropColumn(
                name: "GiftCardIsRedeemed",
                table: "ReferralRewards");

            migrationBuilder.DropColumn(
                name: "GiftCardRedeemedAt",
                table: "ReferralRewards");

            migrationBuilder.AddColumn<int>(
                name: "PointsEarned",
                table: "ReferralRewards",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "Invoices",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PointsEarned",
                table: "ReferralRewards");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "Invoices");

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "ReferralRewards",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "GiftCardCode",
                table: "ReferralRewards",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "GiftCardIsRedeemed",
                table: "ReferralRewards",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "GiftCardRedeemedAt",
                table: "ReferralRewards",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
