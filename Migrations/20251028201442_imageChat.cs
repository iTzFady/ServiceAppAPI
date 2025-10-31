using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceApp.Migrations
{
    /// <inheritdoc />
    public partial class imageChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isImage",
                table: "ChatMessages",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isImage",
                table: "ChatMessages");
        }
    }
}
