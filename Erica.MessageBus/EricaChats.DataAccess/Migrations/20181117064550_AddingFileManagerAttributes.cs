using Microsoft.EntityFrameworkCore.Migrations;

namespace EricaChats.DataAccess.Migrations
{
    public partial class AddingFileManagerAttributes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileAttachmentGUID",
                table: "ChatMessages",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FriendlyFileName",
                table: "ChatMessages",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileAttachmentGUID",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "FriendlyFileName",
                table: "ChatMessages");
        }
    }
}
