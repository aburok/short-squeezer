using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockDataLib.Migrations
{
    /// <inheritdoc />
    public partial class AddChartExchangeBorrowFeeDailyTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChartExchangeBorrowFeeDaily",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Open = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    High = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Low = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Close = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Average = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DataPointCount = table.Column<int>(type: "int", nullable: false),
                    MaxAvailable = table.Column<long>(type: "bigint", nullable: false),
                    MinAvailable = table.Column<long>(type: "bigint", nullable: false),
                    AverageAvailable = table.Column<long>(type: "bigint", nullable: false),
                    ChartExchangeRequestId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StockTickerSymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChartExchangeBorrowFeeDaily", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChartExchangeBorrowFeeDaily_StockTickers_StockTickerSymbol",
                        column: x => x.StockTickerSymbol,
                        principalTable: "StockTickers",
                        principalColumn: "Symbol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChartExchangeBorrowFeeDaily_StockTickerSymbol_Date",
                table: "ChartExchangeBorrowFeeDaily",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChartExchangeBorrowFeeDaily");
        }
    }
}
