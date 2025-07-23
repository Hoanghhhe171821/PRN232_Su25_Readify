using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN232_Su25_Readify_WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateKeyForRecentRead : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Drop key cũ
            migrationBuilder.DropPrimaryKey(
                name: "PK_RecentRead",
                table: "RecentRead");

            migrationBuilder.DropIndex(
                name: "IX_RecentRead_UserId",
                table: "RecentRead");

            // 2. Drop cột Id cũ
            migrationBuilder.DropColumn(
                name: "Id",
                table: "RecentRead");

            // 3. Thêm lại cột Id với Identity
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "RecentRead",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            // 4. Thêm lại primary key mới
            migrationBuilder.AddPrimaryKey(
                name: "PK_RecentRead",
                table: "RecentRead",
                column: "Id");

            // 5. Thêm lại index cần thiết
            migrationBuilder.CreateIndex(
                name: "IX_RecentRead_BookId",
                table: "RecentRead",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_RecentRead_UserId_BookId_ChapterId",
                table: "RecentRead",
                columns: new[] { "UserId", "BookId", "ChapterId" },
                unique: true,
                filter: "[ChapterId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Drop primary key mới
            migrationBuilder.DropPrimaryKey(
                name: "PK_RecentRead",
                table: "RecentRead");

            // 2. Drop các index mới
            migrationBuilder.DropIndex(
                name: "IX_RecentRead_BookId",
                table: "RecentRead");

            migrationBuilder.DropIndex(
                name: "IX_RecentRead_UserId_BookId_ChapterId",
                table: "RecentRead");

            // 3. Drop cột Id có Identity
            migrationBuilder.DropColumn(
                name: "Id",
                table: "RecentRead");

            // 4. Thêm lại cột Id thường
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "RecentRead",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // 5. Thêm lại primary key cũ (composite)
            migrationBuilder.AddPrimaryKey(
                name: "PK_RecentRead",
                table: "RecentRead",
                columns: new[] { "BookId", "UserId" });

            // 6. Thêm lại index cũ
            migrationBuilder.CreateIndex(
                name: "IX_RecentRead_UserId",
                table: "RecentRead",
                column: "UserId");
        }
    }

}
