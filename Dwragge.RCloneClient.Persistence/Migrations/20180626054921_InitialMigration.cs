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
                    RemoteName = table.Column<string>(nullable: false),
                    RemoteBaseFolder = table.Column<string>(nullable: true),
                    SyncTimeSpan = table.Column<TimeSpan>(nullable: false),
                    SyncTimeMinute = table.Column<int>(nullable: false),
                    SyncTimeHour = table.Column<int>(nullable: false),
                    LastSync = table.Column<DateTime>(nullable: true),
                    Name = table.Column<string>(nullable: false)
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
