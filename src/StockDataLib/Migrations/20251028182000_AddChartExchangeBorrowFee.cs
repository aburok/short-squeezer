using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockDataLib.Migrations
{
    /// <inheritdoc />
    public partial class AddChartExchangeBorrowFee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChartExchangeBorrowFee",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Available = table.Column<long>(type: "bigint", nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Rebate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ChartExchangeRequestId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StockTickerSymbol = table.Column<string>(type: "nvarchar(10)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChartExchangeBorrowFee", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChartExchangeBorrowFee_StockTickers_StockTickerSymbol",
                        column: x => x.StockTickerSymbol,
                        principalTable: "StockTickers",
                        principalColumn: "Symbol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChartExchangeBorrowFee_StockTickerSymbol",
                table: "ChartExchangeBorrowFee",
                column: "StockTickerSymbol");

            migrationBuilder.CreateIndex(
                name: "IX_ChartExchangeBorrowFee_StockTickerSymbol_Date",
                table: "ChartExchangeBorrowFee",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChartExchangeBorrowFee");
        }
    }
}
