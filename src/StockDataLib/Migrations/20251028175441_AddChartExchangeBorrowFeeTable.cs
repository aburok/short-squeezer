using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockDataLib.Migrations
{
    /// <inheritdoc />
    public partial class AddChartExchangeBorrowFeeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChartExchangeShortVolume_StockTickerSymbol",
                table: "ChartExchangeShortVolume");

            migrationBuilder.DropIndex(
                name: "IX_ChartExchangeShortInterest_StockTickerSymbol",
                table: "ChartExchangeShortInterest");

            migrationBuilder.DropIndex(
                name: "IX_ChartExchangeBorrowFee_StockTickerSymbol",
                table: "ChartExchangeBorrowFee");

            migrationBuilder.CreateIndex(
                name: "IX_ChartExchangeShortVolume_StockTickerSymbol_Date",
                table: "ChartExchangeShortVolume",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChartExchangeShortInterest_StockTickerSymbol_Date",
                table: "ChartExchangeShortInterest",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChartExchangeShortVolume_StockTickerSymbol_Date",
                table: "ChartExchangeShortVolume");

            migrationBuilder.DropIndex(
                name: "IX_ChartExchangeShortInterest_StockTickerSymbol_Date",
                table: "ChartExchangeShortInterest");

            migrationBuilder.CreateIndex(
                name: "IX_ChartExchangeShortVolume_StockTickerSymbol",
                table: "ChartExchangeShortVolume",
                column: "StockTickerSymbol");

            migrationBuilder.CreateIndex(
                name: "IX_ChartExchangeShortInterest_StockTickerSymbol",
                table: "ChartExchangeShortInterest",
                column: "StockTickerSymbol");

            migrationBuilder.CreateIndex(
                name: "IX_ChartExchangeBorrowFee_StockTickerSymbol",
                table: "ChartExchangeBorrowFee",
                column: "StockTickerSymbol");
        }
    }
}
