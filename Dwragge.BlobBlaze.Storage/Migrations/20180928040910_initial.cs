using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dwragge.BlobBlaze.Storage.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BackupRemotes",
                columns: table => new
                {
                    BackupRemoteId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    BaseFolder = table.Column<string>(nullable: true),
                    ConnectionString = table.Column<string>(nullable: true),
                    Default = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackupRemotes", x => x.BackupRemoteId);
                });

            migrationBuilder.CreateTable(
                name: "BackupFolders",
                columns: table => new
                {
                    BackupFolderId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Path = table.Column<string>(nullable: true),
                    RealTimeUpdates = table.Column<bool>(nullable: false),
                    RemoteBaseFolder = table.Column<string>(nullable: true),
                    SyncTimeSpan = table.Column<TimeSpan>(nullable: false),
                    Size = table.Column<long>(nullable: false, defaultValue: -1L)
                        .Annotation("Sqlite:Autoincrement", true),
                    SyncTime = table.Column<string>(nullable: false),
                    LastSync = table.Column<DateTime>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    BackupRemoteId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackupFolders", x => x.BackupFolderId);
                    table.ForeignKey(
                        name: "FK_BackupFolders_BackupRemotes_BackupRemoteId",
                        column: x => x.BackupRemoteId,
                        principalTable: "BackupRemotes",
                        principalColumn: "BackupRemoteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BackupJobs",
                columns: table => new
                {
                    BackupFolderJobId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Status = table.Column<string>(nullable: false),
                    NumFiles = table.Column<int>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    BackupFolderId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackupJobs", x => x.BackupFolderJobId);
                    table.ForeignKey(
                        name: "FK_BackupJobs_BackupFolders_BackupFolderId",
                        column: x => x.BackupFolderId,
                        principalTable: "BackupFolders",
                        principalColumn: "BackupFolderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackedFiles",
                columns: table => new
                {
                    TrackedFileId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(nullable: true),
                    RemoteLocation = table.Column<string>(nullable: true),
                    FirstTracked = table.Column<DateTime>(nullable: false),
                    LastModified = table.Column<DateTime>(nullable: false),
                    SizeBytes = table.Column<long>(nullable: false),
                    BackupFolderId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackedFiles", x => x.TrackedFileId);
                    table.ForeignKey(
                        name: "FK_TrackedFiles_BackupFolders_BackupFolderId",
                        column: x => x.BackupFolderId,
                        principalTable: "BackupFolders",
                        principalColumn: "BackupFolderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UploadJobs",
                columns: table => new
                {
                    BackupFileUploadJobId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LocalFile = table.Column<string>(nullable: true),
                    ParentJobId = table.Column<int>(nullable: false),
                    Status = table.Column<string>(nullable: false),
                    RetryCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadJobs", x => x.BackupFileUploadJobId);
                    table.ForeignKey(
                        name: "FK_UploadJobs_BackupJobs_ParentJobId",
                        column: x => x.ParentJobId,
                        principalTable: "BackupJobs",
                        principalColumn: "BackupFolderJobId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackedFileVersions",
                columns: table => new
                {
                    TrackedFileVersionId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TrackedFileId = table.Column<int>(nullable: false),
                    RemoteLocation = table.Column<string>(nullable: true),
                    VersionedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackedFileVersions", x => x.TrackedFileVersionId);
                    table.ForeignKey(
                        name: "FK_TrackedFileVersions_TrackedFiles_TrackedFileId",
                        column: x => x.TrackedFileId,
                        principalTable: "TrackedFiles",
                        principalColumn: "TrackedFileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BackupFolders_BackupRemoteId",
                table: "BackupFolders",
                column: "BackupRemoteId");

            migrationBuilder.CreateIndex(
                name: "IX_BackupJobs_BackupFolderId",
                table: "BackupJobs",
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

            migrationBuilder.CreateIndex(
                name: "IX_TrackedFileVersions_TrackedFileId",
                table: "TrackedFileVersions",
                column: "TrackedFileId");

            migrationBuilder.CreateIndex(
                name: "IX_UploadJobs_ParentJobId",
                table: "UploadJobs",
                column: "ParentJobId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrackedFileVersions");

            migrationBuilder.DropTable(
                name: "UploadJobs");

            migrationBuilder.DropTable(
                name: "TrackedFiles");

            migrationBuilder.DropTable(
                name: "BackupJobs");

            migrationBuilder.DropTable(
                name: "BackupFolders");

            migrationBuilder.DropTable(
                name: "BackupRemotes");
        }
    }
}
