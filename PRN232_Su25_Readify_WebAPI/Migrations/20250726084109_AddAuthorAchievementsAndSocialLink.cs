using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN232_Su25_Readify_WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthorAchievementsAndSocialLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuthorAchievement_Authors_AuthorId",
                table: "AuthorAchievement");

            migrationBuilder.DropForeignKey(
                name: "FK_SocialLink_Authors_AuthorId",
                table: "SocialLink");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SocialLink",
                table: "SocialLink");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AuthorAchievement",
                table: "AuthorAchievement");

            migrationBuilder.RenameTable(
                name: "SocialLink",
                newName: "SocialLinks");

            migrationBuilder.RenameTable(
                name: "AuthorAchievement",
                newName: "AuthorAchievements");

            migrationBuilder.RenameIndex(
                name: "IX_SocialLink_AuthorId",
                table: "SocialLinks",
                newName: "IX_SocialLinks_AuthorId");

            migrationBuilder.RenameIndex(
                name: "IX_AuthorAchievement_AuthorId",
                table: "AuthorAchievements",
                newName: "IX_AuthorAchievements_AuthorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SocialLinks",
                table: "SocialLinks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuthorAchievements",
                table: "AuthorAchievements",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AuthorAchievements_Authors_AuthorId",
                table: "AuthorAchievements",
                column: "AuthorId",
                principalTable: "Authors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SocialLinks_Authors_AuthorId",
                table: "SocialLinks",
                column: "AuthorId",
                principalTable: "Authors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuthorAchievements_Authors_AuthorId",
                table: "AuthorAchievements");

            migrationBuilder.DropForeignKey(
                name: "FK_SocialLinks_Authors_AuthorId",
                table: "SocialLinks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SocialLinks",
                table: "SocialLinks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AuthorAchievements",
                table: "AuthorAchievements");

            migrationBuilder.RenameTable(
                name: "SocialLinks",
                newName: "SocialLink");

            migrationBuilder.RenameTable(
                name: "AuthorAchievements",
                newName: "AuthorAchievement");

            migrationBuilder.RenameIndex(
                name: "IX_SocialLinks_AuthorId",
                table: "SocialLink",
                newName: "IX_SocialLink_AuthorId");

            migrationBuilder.RenameIndex(
                name: "IX_AuthorAchievements_AuthorId",
                table: "AuthorAchievement",
                newName: "IX_AuthorAchievement_AuthorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SocialLink",
                table: "SocialLink",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuthorAchievement",
                table: "AuthorAchievement",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AuthorAchievement_Authors_AuthorId",
                table: "AuthorAchievement",
                column: "AuthorId",
                principalTable: "Authors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SocialLink_Authors_AuthorId",
                table: "SocialLink",
                column: "AuthorId",
                principalTable: "Authors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
