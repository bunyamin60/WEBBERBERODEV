using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEBBERBERODEV.Migrations
{
    /// <inheritdoc />
    public partial class uPdataCalisanHizmetModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CalisanHizmetler_Hizmetler_HizmetId1",
                table: "CalisanHizmetler");

            migrationBuilder.DropIndex(
                name: "IX_CalisanHizmetler_HizmetId1",
                table: "CalisanHizmetler");

            migrationBuilder.DropColumn(
                name: "HizmetId1",
                table: "CalisanHizmetler");

            migrationBuilder.AlterColumn<int>(
                name: "HizmetId",
                table: "CalisanHizmetler",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "CalisanId",
                table: "CalisanHizmetler",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "HizmetId",
                table: "CalisanHizmetler",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "CalisanId",
                table: "CalisanHizmetler",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 0);

            migrationBuilder.AddColumn<int>(
                name: "HizmetId1",
                table: "CalisanHizmetler",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CalisanHizmetler_HizmetId1",
                table: "CalisanHizmetler",
                column: "HizmetId1");

            migrationBuilder.AddForeignKey(
                name: "FK_CalisanHizmetler_Hizmetler_HizmetId1",
                table: "CalisanHizmetler",
                column: "HizmetId1",
                principalTable: "Hizmetler",
                principalColumn: "Id");
        }
    }
}
