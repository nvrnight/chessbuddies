using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace ChessBuddies.Migrations
{
    public partial class stats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "game_stats",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    challenged = table.Column<long>(nullable: false),
                    challenger = table.Column<long>(nullable: false),
                    length_seconds = table.Column<long>(nullable: false),
                    total_moves = table.Column<long>(nullable: false),
                    winner = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_stats", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "game_stats",
                schema: "public");
        }
    }
}
