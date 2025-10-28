using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockDataLib.Migrations
{
    /// <inheritdoc />
    public partial class AddPolygonShortVolumeDetailedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AdfShortVolume",
                table: "PolygonShortVolumeData",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "AdfShortVolumeExempt",
                table: "PolygonShortVolumeData",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ExemptVolume",
                table: "PolygonShortVolumeData",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "NasdaqCarteretShortVolume",
                table: "PolygonShortVolumeData",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "NasdaqCarteretShortVolumeExempt",
                table: "PolygonShortVolumeData",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "NasdaqChicagoShortVolume",
                table: "PolygonShortVolumeData",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "NasdaqChicagoShortVolumeExempt",
                table: "PolygonShortVolumeData",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "NonExemptVolume",
                table: "PolygonShortVolumeData",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "NyseShortVolume",
                table: "PolygonShortVolumeData",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "NyseShortVolumeExempt",
                table: "PolygonShortVolumeData",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdfShortVolume",
                table: "PolygonShortVolumeData");

            migrationBuilder.DropColumn(
                name: "AdfShortVolumeExempt",
                table: "PolygonShortVolumeData");

            migrationBuilder.DropColumn(
                name: "ExemptVolume",
                table: "PolygonShortVolumeData");

            migrationBuilder.DropColumn(
                name: "NasdaqCarteretShortVolume",
                table: "PolygonShortVolumeData");

            migrationBuilder.DropColumn(
                name: "NasdaqCarteretShortVolumeExempt",
                table: "PolygonShortVolumeData");

            migrationBuilder.DropColumn(
                name: "NasdaqChicagoShortVolume",
                table: "PolygonShortVolumeData");

            migrationBuilder.DropColumn(
                name: "NasdaqChicagoShortVolumeExempt",
                table: "PolygonShortVolumeData");

            migrationBuilder.DropColumn(
                name: "NonExemptVolume",
                table: "PolygonShortVolumeData");

            migrationBuilder.DropColumn(
                name: "NyseShortVolume",
                table: "PolygonShortVolumeData");

            migrationBuilder.DropColumn(
                name: "NyseShortVolumeExempt",
                table: "PolygonShortVolumeData");
        }
    }
}
