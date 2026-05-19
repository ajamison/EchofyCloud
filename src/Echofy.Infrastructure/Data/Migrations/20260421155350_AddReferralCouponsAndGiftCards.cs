using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Echofy.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReferralCouponsAndGiftCards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WelcomeCouponCode",
                table: "ReferralUses",
                type: "text",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WelcomeCouponCode",
                table: "ReferralUses");

            migrationBuilder.DropColumn(
                name: "GiftCardCode",
                table: "ReferralRewards");

            migrationBuilder.DropColumn(
                name: "GiftCardIsRedeemed",
                table: "ReferralRewards");

            migrationBuilder.DropColumn(
                name: "GiftCardRedeemedAt",
                table: "ReferralRewards");
        }
    }
}
