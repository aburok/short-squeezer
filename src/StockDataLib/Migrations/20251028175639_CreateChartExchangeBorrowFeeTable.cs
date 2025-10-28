using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockDataLib.Migrations
{
    /// <inheritdoc />
    public partial class CreateChartExchangeBorrowFeeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockTickers",
                columns: table => new
                {
                    Symbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Exchange = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTickers", x => x.Symbol);
                });

            migrationBuilder.CreateTable(
                name: "BorrowFeeData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AvailableShares = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    StockTickerSymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BorrowFeeData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BorrowFeeData_StockTickers_StockTickerSymbol",
                        column: x => x.StockTickerSymbol,
                        principalTable: "StockTickers",
                        principalColumn: "Symbol",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    StockTickerSymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
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
                name: "ChartExchangeShortInterest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShortInterestPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShortPosition = table.Column<long>(type: "bigint", nullable: false),
                    DaysToCover = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ChangeNumber = table.Column<long>(type: "bigint", nullable: false),
                    ChangePercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ChartExchangeRequestId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StockTickerSymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
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
                    Rt = table.Column<long>(type: "bigint", nullable: false),
                    St = table.Column<long>(type: "bigint", nullable: false),
                    Lt = table.Column<long>(type: "bigint", nullable: false),
                    Fs = table.Column<long>(type: "bigint", nullable: false),
                    Fse = table.Column<long>(type: "bigint", nullable: false),
                    Xnas = table.Column<long>(type: "bigint", nullable: false),
                    Xphl = table.Column<long>(type: "bigint", nullable: false),
                    Xnys = table.Column<long>(type: "bigint", nullable: false),
                    Arcx = table.Column<long>(type: "bigint", nullable: false),
                    Xcis = table.Column<long>(type: "bigint", nullable: false),
                    Xase = table.Column<long>(type: "bigint", nullable: false),
                    Xchi = table.Column<long>(type: "bigint", nullable: false),
                    Edgx = table.Column<long>(type: "bigint", nullable: false),
                    Bats = table.Column<long>(type: "bigint", nullable: false),
                    Edga = table.Column<long>(type: "bigint", nullable: false),
                    Baty = table.Column<long>(type: "bigint", nullable: false),
                    ShortVolumePercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ChartExchangeRequestId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StockTickerSymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
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

            migrationBuilder.CreateTable(
                name: "FinraShortInterestData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShortInterest = table.Column<long>(type: "bigint", nullable: false),
                    ShortInterestPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MarketValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SharesOutstanding = table.Column<long>(type: "bigint", nullable: false),
                    AvgDailyVolume = table.Column<long>(type: "bigint", nullable: false),
                    Days2Cover = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SettlementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StockTickerSymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinraShortInterestData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinraShortInterestData_StockTickers_StockTickerSymbol",
                        column: x => x.StockTickerSymbol,
                        principalTable: "StockTickers",
                        principalColumn: "Symbol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PriceData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Open = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    High = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Low = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Close = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StockTickerSymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceData_StockTickers_StockTickerSymbol",
                        column: x => x.StockTickerSymbol,
                        principalTable: "StockTickers",
                        principalColumn: "Symbol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RedditMentionData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Mentions = table.Column<int>(type: "int", nullable: false),
                    TopSubreddit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sentiment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StockTickerSymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedditMentionData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RedditMentionData_StockTickers_StockTickerSymbol",
                        column: x => x.StockTickerSymbol,
                        principalTable: "StockTickers",
                        principalColumn: "Symbol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShortInterestData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShortInterest = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SharesShort = table.Column<long>(type: "bigint", nullable: false),
                    StockTickerSymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShortInterestData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShortInterestData_StockTickers_StockTickerSymbol",
                        column: x => x.StockTickerSymbol,
                        principalTable: "StockTickers",
                        principalColumn: "Symbol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShortPositionData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PositionChange = table.Column<long>(type: "bigint", nullable: false),
                    StockTickerSymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShortPositionData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShortPositionData_StockTickers_StockTickerSymbol",
                        column: x => x.StockTickerSymbol,
                        principalTable: "StockTickers",
                        principalColumn: "Symbol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShortVolumeData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShortVolume = table.Column<long>(type: "bigint", nullable: false),
                    ShortVolumePercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StockTickerSymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShortVolumeData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShortVolumeData_StockTickers_StockTickerSymbol",
                        column: x => x.StockTickerSymbol,
                        principalTable: "StockTickers",
                        principalColumn: "Symbol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VolumeData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Volume = table.Column<long>(type: "bigint", nullable: false),
                    StockTickerSymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VolumeData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VolumeData_StockTickers_StockTickerSymbol",
                        column: x => x.StockTickerSymbol,
                        principalTable: "StockTickers",
                        principalColumn: "Symbol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BorrowFeeData_StockTickerSymbol_Date",
                table: "BorrowFeeData",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChartExchangeBorrowFee_StockTickerSymbol_Date",
                table: "ChartExchangeBorrowFee",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);

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
                name: "IX_ChartExchangeRedditMentions_StockTickerSymbol_Date",
                table: "ChartExchangeRedditMentions",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChartExchangeShortInterest_StockTickerSymbol_Date",
                table: "ChartExchangeShortInterest",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChartExchangeShortVolume_StockTickerSymbol_Date",
                table: "ChartExchangeShortVolume",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChartExchangeStockSplit_StockTickerSymbol_Date",
                table: "ChartExchangeStockSplit",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinraShortInterestData_StockTickerSymbol_Date",
                table: "FinraShortInterestData",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceData_StockTickerSymbol_Date",
                table: "PriceData",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RedditMentionData_StockTickerSymbol_Date",
                table: "RedditMentionData",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShortInterestData_StockTickerSymbol_Date",
                table: "ShortInterestData",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShortPositionData_StockTickerSymbol_Date",
                table: "ShortPositionData",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShortVolumeData_StockTickerSymbol_Date",
                table: "ShortVolumeData",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VolumeData_StockTickerSymbol_Date",
                table: "VolumeData",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BorrowFeeData");

            migrationBuilder.DropTable(
                name: "ChartExchangeBorrowFee");

            migrationBuilder.DropTable(
                name: "ChartExchangeFailureToDeliver");

            migrationBuilder.DropTable(
                name: "ChartExchangeOptionChain");

            migrationBuilder.DropTable(
                name: "ChartExchangeRedditMentions");

            migrationBuilder.DropTable(
                name: "ChartExchangeShortInterest");

            migrationBuilder.DropTable(
                name: "ChartExchangeShortVolume");

            migrationBuilder.DropTable(
                name: "ChartExchangeStockSplit");

            migrationBuilder.DropTable(
                name: "FinraShortInterestData");

            migrationBuilder.DropTable(
                name: "PriceData");

            migrationBuilder.DropTable(
                name: "RedditMentionData");

            migrationBuilder.DropTable(
                name: "ShortInterestData");

            migrationBuilder.DropTable(
                name: "ShortPositionData");

            migrationBuilder.DropTable(
                name: "ShortVolumeData");

            migrationBuilder.DropTable(
                name: "VolumeData");

            migrationBuilder.DropTable(
                name: "StockTickers");
        }
    }
}
