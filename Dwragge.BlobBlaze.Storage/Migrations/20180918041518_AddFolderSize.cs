using Microsoft.EntityFrameworkCore.Migrations;

namespace Dwragge.BlobBlaze.Storage.Migrations
{
    public partial class AddFolderSize : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Size",
                table: "BackupFolders",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Size",
                table: "BackupFolders");
        }
    }
}
