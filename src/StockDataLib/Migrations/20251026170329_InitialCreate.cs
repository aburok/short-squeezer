using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockDataLib.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockTickers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Symbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Exchange = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTickers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BorrowFeeData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AvailableShares = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    StockTickerId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BorrowFeeData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BorrowFeeData_StockTickers_StockTickerId",
                        column: x => x.StockTickerId,
                        principalTable: "StockTickers",
                        principalColumn: "Id",
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
                    StockTickerId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceData_StockTickers_StockTickerId",
                        column: x => x.StockTickerId,
                        principalTable: "StockTickers",
                        principalColumn: "Id",
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
                    StockTickerId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedditMentionData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RedditMentionData_StockTickers_StockTickerId",
                        column: x => x.StockTickerId,
                        principalTable: "StockTickers",
                        principalColumn: "Id",
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
                    StockTickerId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShortInterestData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShortInterestData_StockTickers_StockTickerId",
                        column: x => x.StockTickerId,
                        principalTable: "StockTickers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShortPositionData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PositionChange = table.Column<long>(type: "bigint", nullable: false),
                    StockTickerId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShortPositionData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShortPositionData_StockTickers_StockTickerId",
                        column: x => x.StockTickerId,
                        principalTable: "StockTickers",
                        principalColumn: "Id",
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
                    StockTickerId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShortVolumeData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShortVolumeData_StockTickers_StockTickerId",
                        column: x => x.StockTickerId,
                        principalTable: "StockTickers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VolumeData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Volume = table.Column<long>(type: "bigint", nullable: false),
                    StockTickerId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VolumeData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VolumeData_StockTickers_StockTickerId",
                        column: x => x.StockTickerId,
                        principalTable: "StockTickers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BorrowFeeData_StockTickerId_Date",
                table: "BorrowFeeData",
                columns: new[] { "StockTickerId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceData_StockTickerId_Date",
                table: "PriceData",
                columns: new[] { "StockTickerId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RedditMentionData_StockTickerId_Date",
                table: "RedditMentionData",
                columns: new[] { "StockTickerId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShortInterestData_StockTickerId_Date",
                table: "ShortInterestData",
                columns: new[] { "StockTickerId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShortPositionData_StockTickerId_Date",
                table: "ShortPositionData",
                columns: new[] { "StockTickerId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShortVolumeData_StockTickerId_Date",
                table: "ShortVolumeData",
                columns: new[] { "StockTickerId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockTickers_Symbol",
                table: "StockTickers",
                column: "Symbol",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VolumeData_StockTickerId_Date",
                table: "VolumeData",
                columns: new[] { "StockTickerId", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BorrowFeeData");

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
