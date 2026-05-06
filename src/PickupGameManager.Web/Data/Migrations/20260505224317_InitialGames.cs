using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PickupGameManager.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialGames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MaxPlayers = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Games");
        }
    }
}
