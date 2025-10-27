using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockDataLib.Migrations
{
    /// <inheritdoc />
    public partial class AddPolygonShortInterestAndShortVolumeData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PolygonShortInterestData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShortInterest = table.Column<long>(type: "bigint", nullable: false),
                    AvgDailyVolume = table.Column<long>(type: "bigint", nullable: false),
                    DaysToCover = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SettlementDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PolygonRequestId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StockTickerSymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolygonShortInterestData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PolygonShortInterestData_StockTickers_StockTickerSymbol",
                        column: x => x.StockTickerSymbol,
                        principalTable: "StockTickers",
                        principalColumn: "Symbol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PolygonShortVolumeData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShortVolume = table.Column<long>(type: "bigint", nullable: false),
                    TotalVolume = table.Column<long>(type: "bigint", nullable: false),
                    ShortVolumeRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PolygonRequestId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StockTickerSymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolygonShortVolumeData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PolygonShortVolumeData_StockTickers_StockTickerSymbol",
                        column: x => x.StockTickerSymbol,
                        principalTable: "StockTickers",
                        principalColumn: "Symbol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PolygonShortInterestData_StockTickerSymbol_Date",
                table: "PolygonShortInterestData",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PolygonShortVolumeData_StockTickerSymbol_Date",
                table: "PolygonShortVolumeData",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PolygonShortInterestData");

            migrationBuilder.DropTable(
                name: "PolygonShortVolumeData");
        }
    }
}
