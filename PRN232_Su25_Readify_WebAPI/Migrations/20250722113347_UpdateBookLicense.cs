using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN232_Su25_Readify_WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBookLicense : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "OrderItemId",
                table: "BookLicenses",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "BookId",
                table: "BookLicenses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_BookLicenses_BookId",
                table: "BookLicenses",
                column: "BookId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookLicenses_Books_BookId",
                table: "BookLicenses",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookLicenses_Books_BookId",
                table: "BookLicenses");

            migrationBuilder.DropIndex(
                name: "IX_BookLicenses_BookId",
                table: "BookLicenses");

            migrationBuilder.DropColumn(
                name: "BookId",
                table: "BookLicenses");

            migrationBuilder.AlterColumn<int>(
                name: "OrderItemId",
                table: "BookLicenses",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
