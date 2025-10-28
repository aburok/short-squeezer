using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockDataLib.Migrations
{
    /// <inheritdoc />
    public partial class UpdateChartExchangeShortVolumeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Rt",
                table: "ChartExchangeShortVolume",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "St",
                table: "ChartExchangeShortVolume",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Lt",
                table: "ChartExchangeShortVolume",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Fs",
                table: "ChartExchangeShortVolume",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Fse",
                table: "ChartExchangeShortVolume",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Xnas",
                table: "ChartExchangeShortVolume",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Xphl",
                table: "ChartExchangeShortVolume",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Xnys",
                table: "ChartExchangeShortVolume",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Arcx",
                table: "ChartExchangeShortVolume",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Xcis",
                table: "ChartExchangeShortVolume",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Xase",
                table: "ChartExchangeShortVolume",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Xchi",
                table: "ChartExchangeShortVolume",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Edgx",
                table: "ChartExchangeShortVolume",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Bats",
                table: "ChartExchangeShortVolume",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Edga",
                table: "ChartExchangeShortVolume",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Baty",
                table: "ChartExchangeShortVolume",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rt",
                table: "ChartExchangeShortVolume");

            migrationBuilder.DropColumn(
                name: "St",
                table: "ChartExchangeShortVolume");

            migrationBuilder.DropColumn(
                name: "Lt",
                table: "ChartExchangeShortVolume");

            migrationBuilder.DropColumn(
                name: "Fs",
                table: "ChartExchangeShortVolume");

            migrationBuilder.DropColumn(
                name: "Fse",
                table: "ChartExchangeShortVolume");

            migrationBuilder.DropColumn(
                name: "Xnas",
                table: "ChartExchangeShortVolume");

            migrationBuilder.DropColumn(
                name: "Xphl",
                table: "ChartExchangeShortVolume");

            migrationBuilder.DropColumn(
                name: "Xnys",
                table: "ChartExchangeShortVolume");

            migrationBuilder.DropColumn(
                name: "Arcx",
                table: "ChartExchangeShortVolume");

            migrationBuilder.DropColumn(
                name: "Xcis",
                table: "ChartExchangeShortVolume");

            migrationBuilder.DropColumn(
                name: "Xase",
                table: "ChartExchangeShortVolume");

            migrationBuilder.DropColumn(
                name: "Xchi",
                table: "ChartExchangeShortVolume");

            migrationBuilder.DropColumn(
                name: "Edgx",
                table: "ChartExchangeShortVolume");

            migrationBuilder.DropColumn(
                name: "Bats",
                table: "ChartExchangeShortVolume");

            migrationBuilder.DropColumn(
                name: "Edga",
                table: "ChartExchangeShortVolume");

            migrationBuilder.DropColumn(
                name: "Baty",
                table: "ChartExchangeShortVolume");
        }
    }
}
