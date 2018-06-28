using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dwragge.RCloneClient.Persistence.Migrations
{
    public partial class AddBackupTracking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BackedUpFiles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(nullable: false),
                    FirstBackedUp = table.Column<DateTime>(nullable: false),
                    LastModified = table.Column<DateTime>(nullable: false),
                    ParentFolder = table.Column<string>(nullable: false),
                    RemoteLocation = table.Column<string>(nullable: false),
                    IsArchived = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackedUpFiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BackedUpFiles_FileName",
                table: "BackedUpFiles",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_BackedUpFiles_FileName_RemoteLocation",
                table: "BackedUpFiles",
                columns: new[] { "FileName", "RemoteLocation" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BackedUpFiles");
        }
    }
}
