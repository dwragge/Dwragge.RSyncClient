using Microsoft.EntityFrameworkCore.Migrations;

namespace Dwragge.RCloneClient.Persistence.Migrations
{
    public partial class Remotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Remotes",
                columns: table => new
                {
                    RemoteId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: false),
                    ConnectionString = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Remotes", x => x.RemoteId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Remotes");
        }
    }
}
