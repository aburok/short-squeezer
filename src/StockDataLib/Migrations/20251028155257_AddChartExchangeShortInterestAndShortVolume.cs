using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockDataLib.Migrations
{
    /// <inheritdoc />
    public partial class AddChartExchangeShortInterestAndShortVolume : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChartExchangeShortInterest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShortInterest = table.Column<long>(type: "bigint", nullable: false),
                    SharesShort = table.Column<long>(type: "bigint", nullable: false),
                    ShortInterestPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SettlementDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ChartExchangeRequestId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StockTickerSymbol = table.Column<string>(type: "nvarchar(10)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChartExchangeShortInterest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChartExchangeShortInterest_StockTickers_StockTickerSymbol",
                        column: x => x.StockTickerSymbol,
                        principalTable: "StockTickers",
                        principalColumn: "Symbol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChartExchangeShortVolume",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShortVolume = table.Column<long>(type: "bigint", nullable: false),
                    TotalVolume = table.Column<long>(type: "bigint", nullable: false),
                    ShortVolumePercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ChartExchangeRequestId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StockTickerSymbol = table.Column<string>(type: "nvarchar(10)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChartExchangeShortVolume", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChartExchangeShortVolume_StockTickers_StockTickerSymbol",
                        column: x => x.StockTickerSymbol,
                        principalTable: "StockTickers",
                        principalColumn: "Symbol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChartExchangeShortInterest_StockTickerSymbol",
                table: "ChartExchangeShortInterest",
                column: "StockTickerSymbol");

            migrationBuilder.CreateIndex(
                name: "IX_ChartExchangeShortVolume_StockTickerSymbol",
                table: "ChartExchangeShortVolume",
                column: "StockTickerSymbol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChartExchangeShortInterest");

            migrationBuilder.DropTable(
                name: "ChartExchangeShortVolume");
        }
    }
}
