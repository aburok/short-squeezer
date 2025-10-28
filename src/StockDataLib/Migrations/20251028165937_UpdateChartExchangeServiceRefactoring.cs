using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockDataLib.Migrations
{
    /// <inheritdoc />
    public partial class UpdateChartExchangeServiceRefactoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChartExchangePrice");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChartExchangePrice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockTickerSymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AdjustedClose = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ChartExchangeRequestId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Close = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DividendAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    High = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Low = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Open = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SplitCoefficient = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Volume = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChartExchangePrice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChartExchangePrice_StockTickers_StockTickerSymbol",
                        column: x => x.StockTickerSymbol,
                        principalTable: "StockTickers",
                        principalColumn: "Symbol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChartExchangePrice_StockTickerSymbol_Date",
                table: "ChartExchangePrice",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);
        }
    }
}
