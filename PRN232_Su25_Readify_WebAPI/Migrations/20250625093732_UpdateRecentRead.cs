using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN232_Su25_Readify_WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRecentRead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChapterId",
                table: "RecentRead",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateRead",
                table: "RecentRead",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_RecentRead_ChapterId",
                table: "RecentRead",
                column: "ChapterId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecentRead_Chapters_ChapterId",
                table: "RecentRead",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecentRead_Chapters_ChapterId",
                table: "RecentRead");

            migrationBuilder.DropIndex(
                name: "IX_RecentRead_ChapterId",
                table: "RecentRead");

            migrationBuilder.DropColumn(
                name: "ChapterId",
                table: "RecentRead");

            migrationBuilder.DropColumn(
                name: "DateRead",
                table: "RecentRead");
        }
    }
}
