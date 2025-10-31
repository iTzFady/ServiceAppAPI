using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedServiceRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Region",
                table: "ServiceRequests",
                newName: "title");

            migrationBuilder.AddColumn<DateTime>(
                name: "dateTime",
                table: "ServiceRequests",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "location",
                table: "ServiceRequests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "notes",
                table: "ServiceRequests",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "dateTime",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "location",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "notes",
                table: "ServiceRequests");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "ServiceRequests",
                newName: "Region");
        }
    }
}
