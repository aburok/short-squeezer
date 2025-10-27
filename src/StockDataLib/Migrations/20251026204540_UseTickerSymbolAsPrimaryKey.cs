using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockDataLib.Migrations
{
    /// <inheritdoc />
    public partial class UseTickerSymbolAsPrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BorrowFeeData_StockTickers_StockTickerId",
                table: "BorrowFeeData");

            migrationBuilder.DropForeignKey(
                name: "FK_PriceData_StockTickers_StockTickerId",
                table: "PriceData");

            migrationBuilder.DropForeignKey(
                name: "FK_RedditMentionData_StockTickers_StockTickerId",
                table: "RedditMentionData");

            migrationBuilder.DropForeignKey(
                name: "FK_ShortInterestData_StockTickers_StockTickerId",
                table: "ShortInterestData");

            migrationBuilder.DropForeignKey(
                name: "FK_ShortPositionData_StockTickers_StockTickerId",
                table: "ShortPositionData");

            migrationBuilder.DropForeignKey(
                name: "FK_ShortVolumeData_StockTickers_StockTickerId",
                table: "ShortVolumeData");

            migrationBuilder.DropForeignKey(
                name: "FK_VolumeData_StockTickers_StockTickerId",
                table: "VolumeData");

            migrationBuilder.DropIndex(
                name: "IX_VolumeData_StockTickerId_Date",
                table: "VolumeData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StockTickers",
                table: "StockTickers");

            migrationBuilder.DropIndex(
                name: "IX_StockTickers_Symbol",
                table: "StockTickers");

            migrationBuilder.DropIndex(
                name: "IX_ShortVolumeData_StockTickerId_Date",
                table: "ShortVolumeData");

            migrationBuilder.DropIndex(
                name: "IX_ShortPositionData_StockTickerId_Date",
                table: "ShortPositionData");

            migrationBuilder.DropIndex(
                name: "IX_ShortInterestData_StockTickerId_Date",
                table: "ShortInterestData");

            migrationBuilder.DropIndex(
                name: "IX_RedditMentionData_StockTickerId_Date",
                table: "RedditMentionData");

            migrationBuilder.DropIndex(
                name: "IX_PriceData_StockTickerId_Date",
                table: "PriceData");

            migrationBuilder.DropIndex(
                name: "IX_BorrowFeeData_StockTickerId_Date",
                table: "BorrowFeeData");

            migrationBuilder.DropColumn(
                name: "StockTickerId",
                table: "VolumeData");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "StockTickers");

            migrationBuilder.DropColumn(
                name: "StockTickerId",
                table: "ShortVolumeData");

            migrationBuilder.DropColumn(
                name: "StockTickerId",
                table: "ShortPositionData");

            migrationBuilder.DropColumn(
                name: "StockTickerId",
                table: "ShortInterestData");

            migrationBuilder.DropColumn(
                name: "StockTickerId",
                table: "RedditMentionData");

            migrationBuilder.DropColumn(
                name: "StockTickerId",
                table: "PriceData");

            migrationBuilder.DropColumn(
                name: "StockTickerId",
                table: "BorrowFeeData");

            migrationBuilder.AddColumn<string>(
                name: "StockTickerSymbol",
                table: "VolumeData",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StockTickerSymbol",
                table: "ShortVolumeData",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StockTickerSymbol",
                table: "ShortPositionData",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StockTickerSymbol",
                table: "ShortInterestData",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StockTickerSymbol",
                table: "RedditMentionData",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StockTickerSymbol",
                table: "PriceData",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StockTickerSymbol",
                table: "BorrowFeeData",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StockTickers",
                table: "StockTickers",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_VolumeData_StockTickerSymbol_Date",
                table: "VolumeData",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShortVolumeData_StockTickerSymbol_Date",
                table: "ShortVolumeData",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShortPositionData_StockTickerSymbol_Date",
                table: "ShortPositionData",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShortInterestData_StockTickerSymbol_Date",
                table: "ShortInterestData",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RedditMentionData_StockTickerSymbol_Date",
                table: "RedditMentionData",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceData_StockTickerSymbol_Date",
                table: "PriceData",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BorrowFeeData_StockTickerSymbol_Date",
                table: "BorrowFeeData",
                columns: new[] { "StockTickerSymbol", "Date" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowFeeData_StockTickers_StockTickerSymbol",
                table: "BorrowFeeData",
                column: "StockTickerSymbol",
                principalTable: "StockTickers",
                principalColumn: "Symbol",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PriceData_StockTickers_StockTickerSymbol",
                table: "PriceData",
                column: "StockTickerSymbol",
                principalTable: "StockTickers",
                principalColumn: "Symbol",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RedditMentionData_StockTickers_StockTickerSymbol",
                table: "RedditMentionData",
                column: "StockTickerSymbol",
                principalTable: "StockTickers",
                principalColumn: "Symbol",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShortInterestData_StockTickers_StockTickerSymbol",
                table: "ShortInterestData",
                column: "StockTickerSymbol",
                principalTable: "StockTickers",
                principalColumn: "Symbol",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShortPositionData_StockTickers_StockTickerSymbol",
                table: "ShortPositionData",
                column: "StockTickerSymbol",
                principalTable: "StockTickers",
                principalColumn: "Symbol",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShortVolumeData_StockTickers_StockTickerSymbol",
                table: "ShortVolumeData",
                column: "StockTickerSymbol",
                principalTable: "StockTickers",
                principalColumn: "Symbol",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VolumeData_StockTickers_StockTickerSymbol",
                table: "VolumeData",
                column: "StockTickerSymbol",
                principalTable: "StockTickers",
                principalColumn: "Symbol",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BorrowFeeData_StockTickers_StockTickerSymbol",
                table: "BorrowFeeData");

            migrationBuilder.DropForeignKey(
                name: "FK_PriceData_StockTickers_StockTickerSymbol",
                table: "PriceData");

            migrationBuilder.DropForeignKey(
                name: "FK_RedditMentionData_StockTickers_StockTickerSymbol",
                table: "RedditMentionData");

            migrationBuilder.DropForeignKey(
                name: "FK_ShortInterestData_StockTickers_StockTickerSymbol",
                table: "ShortInterestData");

            migrationBuilder.DropForeignKey(
                name: "FK_ShortPositionData_StockTickers_StockTickerSymbol",
                table: "ShortPositionData");

            migrationBuilder.DropForeignKey(
                name: "FK_ShortVolumeData_StockTickers_StockTickerSymbol",
                table: "ShortVolumeData");

            migrationBuilder.DropForeignKey(
                name: "FK_VolumeData_StockTickers_StockTickerSymbol",
                table: "VolumeData");

            migrationBuilder.DropIndex(
                name: "IX_VolumeData_StockTickerSymbol_Date",
                table: "VolumeData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StockTickers",
                table: "StockTickers");

            migrationBuilder.DropIndex(
                name: "IX_ShortVolumeData_StockTickerSymbol_Date",
                table: "ShortVolumeData");

            migrationBuilder.DropIndex(
                name: "IX_ShortPositionData_StockTickerSymbol_Date",
                table: "ShortPositionData");

            migrationBuilder.DropIndex(
                name: "IX_ShortInterestData_StockTickerSymbol_Date",
                table: "ShortInterestData");

            migrationBuilder.DropIndex(
                name: "IX_RedditMentionData_StockTickerSymbol_Date",
                table: "RedditMentionData");

            migrationBuilder.DropIndex(
                name: "IX_PriceData_StockTickerSymbol_Date",
                table: "PriceData");

            migrationBuilder.DropIndex(
                name: "IX_BorrowFeeData_StockTickerSymbol_Date",
                table: "BorrowFeeData");

            migrationBuilder.DropColumn(
                name: "StockTickerSymbol",
                table: "VolumeData");

            migrationBuilder.DropColumn(
                name: "StockTickerSymbol",
                table: "ShortVolumeData");

            migrationBuilder.DropColumn(
                name: "StockTickerSymbol",
                table: "ShortPositionData");

            migrationBuilder.DropColumn(
                name: "StockTickerSymbol",
                table: "ShortInterestData");

            migrationBuilder.DropColumn(
                name: "StockTickerSymbol",
                table: "RedditMentionData");

            migrationBuilder.DropColumn(
                name: "StockTickerSymbol",
                table: "PriceData");

            migrationBuilder.DropColumn(
                name: "StockTickerSymbol",
                table: "BorrowFeeData");

            migrationBuilder.AddColumn<int>(
                name: "StockTickerId",
                table: "VolumeData",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "StockTickers",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "StockTickerId",
                table: "ShortVolumeData",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StockTickerId",
                table: "ShortPositionData",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StockTickerId",
                table: "ShortInterestData",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StockTickerId",
                table: "RedditMentionData",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StockTickerId",
                table: "PriceData",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StockTickerId",
                table: "BorrowFeeData",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_StockTickers",
                table: "StockTickers",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_VolumeData_StockTickerId_Date",
                table: "VolumeData",
                columns: new[] { "StockTickerId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockTickers_Symbol",
                table: "StockTickers",
                column: "Symbol",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShortVolumeData_StockTickerId_Date",
                table: "ShortVolumeData",
                columns: new[] { "StockTickerId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShortPositionData_StockTickerId_Date",
                table: "ShortPositionData",
                columns: new[] { "StockTickerId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShortInterestData_StockTickerId_Date",
                table: "ShortInterestData",
                columns: new[] { "StockTickerId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RedditMentionData_StockTickerId_Date",
                table: "RedditMentionData",
                columns: new[] { "StockTickerId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceData_StockTickerId_Date",
                table: "PriceData",
                columns: new[] { "StockTickerId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BorrowFeeData_StockTickerId_Date",
                table: "BorrowFeeData",
                columns: new[] { "StockTickerId", "Date" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowFeeData_StockTickers_StockTickerId",
                table: "BorrowFeeData",
                column: "StockTickerId",
                principalTable: "StockTickers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PriceData_StockTickers_StockTickerId",
                table: "PriceData",
                column: "StockTickerId",
                principalTable: "StockTickers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RedditMentionData_StockTickers_StockTickerId",
                table: "RedditMentionData",
                column: "StockTickerId",
                principalTable: "StockTickers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShortInterestData_StockTickers_StockTickerId",
                table: "ShortInterestData",
                column: "StockTickerId",
                principalTable: "StockTickers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShortPositionData_StockTickers_StockTickerId",
                table: "ShortPositionData",
                column: "StockTickerId",
                principalTable: "StockTickers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShortVolumeData_StockTickers_StockTickerId",
                table: "ShortVolumeData",
                column: "StockTickerId",
                principalTable: "StockTickers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VolumeData_StockTickers_StockTickerId",
                table: "VolumeData",
                column: "StockTickerId",
                principalTable: "StockTickers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
