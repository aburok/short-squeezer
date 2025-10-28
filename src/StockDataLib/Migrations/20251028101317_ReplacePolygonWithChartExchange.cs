using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockDataLib.Migrations
{
    /// <inheritdoc />
    public partial class ReplacePolygonWithChartExchange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PolygonPriceData");

            migrationBuilder.DropTable(
                name: "PolygonShortInterestData");

            migrationBuilder.DropTable(
                name: "PolygonShortVolumeData");

            migrationBuilder.CreateTable(
                name: "ChartExchangeFailureToDeliver",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FailureToDeliver = table.Column<long>(type: "bigint", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Volume = table.Column<long>(type: "bigint", nullable: false),
                    SettlementDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Cusip = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChartExchangeRequestId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StockTickerSymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChartExchangeFailureToDeliver", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChartExchangeFailureToDeliver_StockTickers_StockTickerSymbol",
                        column: x => x.StockTickerSymbol,
                        principalTable: "StockTickers",
                        principalColumn: "Symbol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChartExchangeOptionChain",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExpirationDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrikePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OptionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Volume = table.Column<long>(type: "bigint", nullable: false),
                    OpenInterest = table.Column<long>(type: "bigint", nullable: false),
                    Bid = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Ask = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LastPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ImpliedVolatility = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Delta = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Gamma = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Theta = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Vega = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ChartExchangeRequestId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StockTickerSymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChartExchangeOptionChain", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChartExchangeOptionChain_StockTickers_StockTickerSymbol",
                        column: x => x.StockTickerSymbol,
                        principalTable: "StockTickers",
                        principalColumn: "Symbol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChartExchangePrice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Open = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    High = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Low = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Close = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Volume = table.Column<long>(type: "bigint", nullable: false),
                    AdjustedClose = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DividendAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SplitCoefficient = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ChartExchangeRequestId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StockTickerSymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "ChartExchangeRedditMentions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Mentions = table.Column<int>(type: "int", nullable: false),
                    SentimentScore = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SentimentLabel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Subreddit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Upvotes = table.Column<int>(type: "int", nullable: true),
                    Comments = table.Column<int>(type: "int", nullable: true),
                    EngagementScore = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ChartExchangeRequestId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StockTickerSymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChartExchangeRedditMentions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChartExchangeRedditMentions_StockTickers_StockTickerSymbol",
                        column: x => x.StockTickerSymbol,
                        principalTable: "StockTickers",
                        principalColumn: "Symbol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChartExchangeStockSplit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SplitRatio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SplitFactor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FromFactor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ToFactor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecordDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PayableDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AnnouncementDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChartExchangeRequestId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StockTickerSymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChartExchangeStockSplit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChartExchangeStockSplit_StockTickers_StockTickerSymbol",
                        column: x => x.StockTickerSymbol,
                        principalTable: "StockTickers",
                        principalColumn: "Symbol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChartExchangeFailureToDeliver_StockTickerSymbol_Date",
                table: "ChartExchangeFailureToDeliver",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChartExchangeOptionChain_StockTickerSymbol_Date",
                table: "ChartExchangeOptionChain",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChartExchangePrice_StockTickerSymbol_Date",
                table: "ChartExchangePrice",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChartExchangeRedditMentions_StockTickerSymbol_Date",
                table: "ChartExchangeRedditMentions",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChartExchangeStockSplit_StockTickerSymbol_Date",
                table: "ChartExchangeStockSplit",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChartExchangeFailureToDeliver");

            migrationBuilder.DropTable(
                name: "ChartExchangeOptionChain");

            migrationBuilder.DropTable(
                name: "ChartExchangePrice");

            migrationBuilder.DropTable(
                name: "ChartExchangeRedditMentions");

            migrationBuilder.DropTable(
                name: "ChartExchangeStockSplit");

            migrationBuilder.CreateTable(
                name: "PolygonPriceData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockTickerSymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Close = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    High = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Low = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NumberOfTransactions = table.Column<int>(type: "int", nullable: true),
                    Open = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PolygonRequestId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Volume = table.Column<long>(type: "bigint", nullable: false),
                    VolumeWeightedPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolygonPriceData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PolygonPriceData_StockTickers_StockTickerSymbol",
                        column: x => x.StockTickerSymbol,
                        principalTable: "StockTickers",
                        principalColumn: "Symbol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PolygonShortInterestData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockTickerSymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AvgDailyVolume = table.Column<long>(type: "bigint", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DaysToCover = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PolygonRequestId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SettlementDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ShortInterest = table.Column<long>(type: "bigint", nullable: false)
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
                    StockTickerSymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AdfShortVolume = table.Column<long>(type: "bigint", nullable: true),
                    AdfShortVolumeExempt = table.Column<long>(type: "bigint", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExemptVolume = table.Column<long>(type: "bigint", nullable: true),
                    NasdaqCarteretShortVolume = table.Column<long>(type: "bigint", nullable: true),
                    NasdaqCarteretShortVolumeExempt = table.Column<long>(type: "bigint", nullable: true),
                    NasdaqChicagoShortVolume = table.Column<long>(type: "bigint", nullable: true),
                    NasdaqChicagoShortVolumeExempt = table.Column<long>(type: "bigint", nullable: true),
                    NonExemptVolume = table.Column<long>(type: "bigint", nullable: true),
                    NyseShortVolume = table.Column<long>(type: "bigint", nullable: true),
                    NyseShortVolumeExempt = table.Column<long>(type: "bigint", nullable: true),
                    PolygonRequestId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShortVolume = table.Column<long>(type: "bigint", nullable: false),
                    ShortVolumeRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalVolume = table.Column<long>(type: "bigint", nullable: false)
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
                name: "IX_PolygonPriceData_StockTickerSymbol_Date",
                table: "PolygonPriceData",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);

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
    }
}
