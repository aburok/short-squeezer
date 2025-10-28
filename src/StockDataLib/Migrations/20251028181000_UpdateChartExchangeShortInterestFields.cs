using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockDataLib.Migrations
{
    /// <inheritdoc />
    public partial class UpdateChartExchangeShortInterestFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop old columns
            migrationBuilder.DropColumn(
                name: "ShortInterest",
                table: "ChartExchangeShortInterest");

            migrationBuilder.DropColumn(
                name: "SharesShort",
                table: "ChartExchangeShortInterest");

            migrationBuilder.DropColumn(
                name: "SettlementDate",
                table: "ChartExchangeShortInterest");

            // Add new columns
            migrationBuilder.AddColumn<long>(
                name: "ShortPosition",
                table: "ChartExchangeShortInterest",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<decimal>(
                name: "DaysToCover",
                table: "ChartExchangeShortInterest",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "ChangeNumber",
                table: "ChartExchangeShortInterest",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<decimal>(
                name: "ChangePercent",
                table: "ChartExchangeShortInterest",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop new columns
            migrationBuilder.DropColumn(
                name: "ShortPosition",
                table: "ChartExchangeShortInterest");

            migrationBuilder.DropColumn(
                name: "DaysToCover",
                table: "ChartExchangeShortInterest");

            migrationBuilder.DropColumn(
                name: "ChangeNumber",
                table: "ChartExchangeShortInterest");

            migrationBuilder.DropColumn(
                name: "ChangePercent",
                table: "ChartExchangeShortInterest");

            // Add back old columns
            migrationBuilder.AddColumn<long>(
                name: "ShortInterest",
                table: "ChartExchangeShortInterest",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "SharesShort",
                table: "ChartExchangeShortInterest",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "SettlementDate",
                table: "ChartExchangeShortInterest",
                type: "datetime2",
                nullable: true);
        }
    }
}
