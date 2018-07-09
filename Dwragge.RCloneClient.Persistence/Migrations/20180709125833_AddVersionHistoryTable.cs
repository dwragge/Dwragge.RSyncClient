using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dwragge.RCloneClient.Persistence.Migrations
{
    public partial class AddVersionHistoryTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BackupFolders",
                columns: table => new
                {
                    BackupFolderId = table.Column<int>(nullable: false)
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
                    table.PrimaryKey("PK_BackupFolders", x => x.BackupFolderId);
                });

            migrationBuilder.CreateTable(
                name: "FileVersionHistory",
                columns: table => new
                {
                    VersionHistoryId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(nullable: false),
                    RemoteLocation = table.Column<string>(nullable: false),
                    VersionedOn = table.Column<DateTime>(nullable: false),
                    BackupFolderId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileVersionHistory", x => x.VersionHistoryId);
                    table.ForeignKey(
                        name: "FK_FileVersionHistory_BackupFolders_BackupFolderId",
                        column: x => x.BackupFolderId,
                        principalTable: "BackupFolders",
                        principalColumn: "BackupFolderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InProgressFiles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BackupFolderId = table.Column<int>(nullable: false),
                    InsertedAt = table.Column<DateTime>(nullable: false),
                    RemotePath = table.Column<string>(nullable: true),
                    FileName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InProgressFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InProgressFiles_BackupFolders_BackupFolderId",
                        column: x => x.BackupFolderId,
                        principalTable: "BackupFolders",
                        principalColumn: "BackupFolderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PendingFiles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(nullable: false),
                    BackupFolderId = table.Column<int>(nullable: false),
                    QueuedTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PendingFiles_BackupFolders_BackupFolderId",
                        column: x => x.BackupFolderId,
                        principalTable: "BackupFolders",
                        principalColumn: "BackupFolderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackedFiles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(nullable: false),
                    RemoteLocation = table.Column<string>(nullable: false),
                    FirstBackedUp = table.Column<DateTime>(nullable: false),
                    LastModified = table.Column<DateTime>(nullable: false),
                    SizeBytes = table.Column<long>(nullable: false),
                    BackupFolderId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackedFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackedFiles_BackupFolders_BackupFolderId",
                        column: x => x.BackupFolderId,
                        principalTable: "BackupFolders",
                        principalColumn: "BackupFolderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileVersionHistory_BackupFolderId",
                table: "FileVersionHistory",
                column: "BackupFolderId");

            migrationBuilder.CreateIndex(
                name: "IX_InProgressFiles_BackupFolderId",
                table: "InProgressFiles",
                column: "BackupFolderId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingFiles_BackupFolderId",
                table: "PendingFiles",
                column: "BackupFolderId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackedFiles_BackupFolderId",
                table: "TrackedFiles",
                column: "BackupFolderId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackedFiles_FileName",
                table: "TrackedFiles",
                column: "FileName",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileVersionHistory");

            migrationBuilder.DropTable(
                name: "InProgressFiles");

            migrationBuilder.DropTable(
                name: "PendingFiles");

            migrationBuilder.DropTable(
                name: "TrackedFiles");

            migrationBuilder.DropTable(
                name: "BackupFolders");
        }
    }
}
