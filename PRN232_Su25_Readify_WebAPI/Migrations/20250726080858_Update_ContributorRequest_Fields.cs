using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN232_Su25_Readify_WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class Update_ContributorRequest_Fields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ContributorRequests",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_ContributorRequests_UserId",
                table: "ContributorRequests",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContributorRequests_AspNetUsers_UserId",
                table: "ContributorRequests",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContributorRequests_AspNetUsers_UserId",
                table: "ContributorRequests");

            migrationBuilder.DropIndex(
                name: "IX_ContributorRequests_UserId",
                table: "ContributorRequests");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ContributorRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
