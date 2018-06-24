using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dwragge.RCloneClient.Persistence.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BackupFolders",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Path = table.Column<string>(nullable: false),
                    RealTimeUpdates = table.Column<bool>(nullable: false),
                    SyncTimeSpan = table.Column<TimeSpan>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackupFolders", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BackupFolders");
        }
    }
}
