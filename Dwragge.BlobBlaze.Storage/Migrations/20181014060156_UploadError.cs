using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dwragge.BlobBlaze.Storage.Migrations
{
    public partial class UploadError : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UploadErrors",
                columns: table => new
                {
                    UploadErrorId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BackupFolderJobId = table.Column<int>(nullable: false),
                    File = table.Column<string>(nullable: true),
                    ExceptionMessage = table.Column<string>(nullable: true),
                    InnerExceptionMessage = table.Column<string>(nullable: true),
                    StackTrace = table.Column<string>(nullable: true),
                    ErrorTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadErrors", x => x.UploadErrorId);
                    table.ForeignKey(
                        name: "FK_UploadErrors_BackupJobs_BackupFolderJobId",
                        column: x => x.BackupFolderJobId,
                        principalTable: "BackupJobs",
                        principalColumn: "BackupFolderJobId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UploadErrors_BackupFolderJobId",
                table: "UploadErrors",
                column: "BackupFolderJobId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UploadErrors");
        }
    }
}
