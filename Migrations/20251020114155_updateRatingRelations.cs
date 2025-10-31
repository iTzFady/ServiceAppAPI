using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceApp.Migrations
{
    /// <inheritdoc />
    public partial class updateRatingRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ratings_Users_RatedByUserId",
                table: "Ratings");

            migrationBuilder.DropForeignKey(
                name: "FK_Ratings_Users_RatedUserId",
                table: "Ratings");

            migrationBuilder.AddColumn<string>(
                name: "profilePictureUrl",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_RequestedByUserId",
                table: "ServiceRequests",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_RequestedForUserId",
                table: "ServiceRequests",
                column: "RequestedForUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_Users_RatedByUserId",
                table: "Ratings",
                column: "RatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_Users_RatedUserId",
                table: "Ratings",
                column: "RatedUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_RequestedByUserId",
                table: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_RequestedForUserId",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "profilePictureUrl",
                table: "Users");

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
        }
    }
}
