using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceApp.Migrations
{
    /// <inheritdoc />
    public partial class bansystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBanned",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportedUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_RequestedByUserId",
                table: "ServiceRequests",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_RequestedForUserId",
                table: "ServiceRequests",
                column: "RequestedForUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_RatedByUserId",
                table: "Ratings",
                column: "RatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_RatedUserId",
                table: "Ratings",
                column: "RatedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_ServiceRequestId",
                table: "Ratings",
                column: "ServiceRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_ServiceRequests_ServiceRequestId",
                table: "Ratings",
                column: "ServiceRequestId",
                principalTable: "ServiceRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_Users_RatedByUserId",
                table: "Ratings",
                column: "RatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_Users_RatedUserId",
                table: "Ratings",
                column: "RatedUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_Users_RequestedByUserId",
                table: "ServiceRequests",
                column: "RequestedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_Users_RequestedForUserId",
                table: "ServiceRequests",
                column: "RequestedForUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ratings_ServiceRequests_ServiceRequestId",
                table: "Ratings");

            migrationBuilder.DropForeignKey(
                name: "FK_Ratings_Users_RatedByUserId",
                table: "Ratings");

            migrationBuilder.DropForeignKey(
                name: "FK_Ratings_Users_RatedUserId",
                table: "Ratings");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_Users_RequestedByUserId",
                table: "ServiceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_Users_RequestedForUserId",
                table: "ServiceRequests");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_RequestedByUserId",
                table: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_RequestedForUserId",
                table: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_Ratings_RatedByUserId",
                table: "Ratings");

            migrationBuilder.DropIndex(
                name: "IX_Ratings_RatedUserId",
                table: "Ratings");

            migrationBuilder.DropIndex(
                name: "IX_Ratings_ServiceRequestId",
                table: "Ratings");

            migrationBuilder.DropColumn(
                name: "IsBanned",
                table: "Users");
        }
    }
}
