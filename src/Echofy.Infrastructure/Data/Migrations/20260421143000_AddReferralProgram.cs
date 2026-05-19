using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Echofy.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReferralProgram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReferralCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferralCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReferralCodes_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReferralUses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReferralCodeId = table.Column<int>(type: "integer", nullable: false),
                    UsedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    UsedByEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferralUses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReferralUses_AspNetUsers_UsedByUserId",
                        column: x => x.UsedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReferralUses_ReferralCodes_ReferralCodeId",
                        column: x => x.ReferralCodeId,
                        principalTable: "ReferralCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReferralRewards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    ReferralUseId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferralRewards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReferralRewards_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReferralRewards_ReferralUses_ReferralUseId",
                        column: x => x.ReferralUseId,
                        principalTable: "ReferralUses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReferralCodes_AppUserId",
                table: "ReferralCodes",
                column: "AppUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReferralCodes_Code",
                table: "ReferralCodes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReferralRewards_AppUserId",
                table: "ReferralRewards",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralRewards_ReferralUseId",
                table: "ReferralRewards",
                column: "ReferralUseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReferralUses_ReferralCodeId",
                table: "ReferralUses",
                column: "ReferralCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralUses_UsedByUserId",
                table: "ReferralUses",
                column: "UsedByUserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReferralRewards");

            migrationBuilder.DropTable(
                name: "ReferralUses");

            migrationBuilder.DropTable(
                name: "ReferralCodes");
        }
    }
}
