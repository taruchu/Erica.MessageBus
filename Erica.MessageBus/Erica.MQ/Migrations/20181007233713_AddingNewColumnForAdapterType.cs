using Microsoft.EntityFrameworkCore.Migrations;

namespace Erica.MQ.Migrations
{
    public partial class AddingNewColumnForAdapterType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdapterAssemblyQualifiedName",
                table: "EricaMQ_Messages",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdapterAssemblyQualifiedName",
                table: "EricaMQ_Messages");
        }
    }
}
