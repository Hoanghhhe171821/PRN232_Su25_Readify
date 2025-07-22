using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN232_Su25_Readify_WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRecentReading : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RevenueSummaries_Authors_AuthorId",
                table: "RevenueSummaries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RevenueSummaries",
                table: "RevenueSummaries");

            migrationBuilder.RenameTable(
                name: "RevenueSummaries",
                newName: "AuthorRevenueSummary");

            migrationBuilder.RenameIndex(
                name: "IX_RevenueSummaries_AuthorId",
                table: "AuthorRevenueSummary",
                newName: "IX_AuthorRevenueSummary_AuthorId");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "RecentRead",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuthorRevenueSummary",
                table: "AuthorRevenueSummary",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "BookLicenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OrderItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookLicenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookLicenses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookLicenses_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookLicenses_OrderItemId",
                table: "BookLicenses",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_BookLicenses_UserId",
                table: "BookLicenses",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuthorRevenueSummary_Authors_AuthorId",
                table: "AuthorRevenueSummary",
                column: "AuthorId",
                principalTable: "Authors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuthorRevenueSummary_Authors_AuthorId",
                table: "AuthorRevenueSummary");

            migrationBuilder.DropTable(
                name: "BookLicenses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AuthorRevenueSummary",
                table: "AuthorRevenueSummary");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "RecentRead");

            migrationBuilder.RenameTable(
                name: "AuthorRevenueSummary",
                newName: "RevenueSummaries");

            migrationBuilder.RenameIndex(
                name: "IX_AuthorRevenueSummary_AuthorId",
                table: "RevenueSummaries",
                newName: "IX_RevenueSummaries_AuthorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RevenueSummaries",
                table: "RevenueSummaries",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RevenueSummaries_Authors_AuthorId",
                table: "RevenueSummaries",
                column: "AuthorId",
                principalTable: "Authors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
