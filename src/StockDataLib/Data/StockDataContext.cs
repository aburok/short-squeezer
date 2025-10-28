using Microsoft.EntityFrameworkCore;
using StockDataLib.Models;

namespace StockDataLib.Data
{
    public class StockDataContext : DbContext
    {
        public StockDataContext(DbContextOptions<StockDataContext> options) : base(options)
        {
        }

        public DbSet<StockTicker> StockTickers { get; set; }
        public DbSet<PriceData> PriceData { get; set; }
        public DbSet<VolumeData> VolumeData { get; set; }
        public DbSet<ShortVolumeData> ShortVolumeData { get; set; }
        public DbSet<ShortPositionData> ShortPositionData { get; set; }
        public DbSet<ShortInterestData> ShortInterestData { get; set; }
        public DbSet<BorrowFeeData> BorrowFeeData { get; set; }
        public DbSet<RedditMentionData> RedditMentionData { get; set; }
        public DbSet<FinraShortInterestData> FinraShortInterestData { get; set; }

        // ChartExchange data sets
        public DbSet<ChartExchangeFailureToDeliver> ChartExchangeFailureToDeliver { get; set; }
        public DbSet<ChartExchangeRedditMentions> ChartExchangeRedditMentions { get; set; }
        public DbSet<ChartExchangeOptionChain> ChartExchangeOptionChain { get; set; }
        public DbSet<ChartExchangeStockSplit> ChartExchangeStockSplit { get; set; }
        public DbSet<ChartExchangeShortInterest> ChartExchangeShortInterest { get; set; }
        public DbSet<ChartExchangeShortVolume> ChartExchangeShortVolume { get; set; }
        public DbSet<ChartExchangeBorrowFee> ChartExchangeBorrowFee { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure StockTicker
            modelBuilder.Entity<StockTicker>()
                .HasKey(s => s.Symbol);

            modelBuilder.Entity<StockTicker>()
                .Property(s => s.Symbol)
                .IsRequired()
                .HasMaxLength(10);

            modelBuilder.Entity<StockTicker>()
                .Property(s => s.Exchange)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<StockTicker>()
                .Property(s => s.Name)
                .HasMaxLength(1000);

            // Configure base StockDataPoint properties
            void ConfigureStockDataPoint<T>(ModelBuilder builder) where T : StockDataPoint
            {
                builder.Entity<T>()
                    .HasKey(d => d.Id);

                builder.Entity<T>()
                    .HasIndex(d => new { d.StockTickerSymbol, d.Date })
                    .IsUnique();

                builder.Entity<T>()
                    .Property(d => d.StockTickerSymbol)
                    .IsRequired()
                    .HasMaxLength(10);

                builder.Entity<T>()
                    .HasOne(d => d.StockTicker)
                    .WithMany()
                    .HasForeignKey(d => d.StockTickerSymbol)
                    .OnDelete(DeleteBehavior.Cascade);
            }

            // Configure each data type
            ConfigureStockDataPoint<PriceData>(modelBuilder);
            ConfigureStockDataPoint<VolumeData>(modelBuilder);
            ConfigureStockDataPoint<ShortVolumeData>(modelBuilder);
            ConfigureStockDataPoint<ShortPositionData>(modelBuilder);
            ConfigureStockDataPoint<ShortInterestData>(modelBuilder);
            ConfigureStockDataPoint<BorrowFeeData>(modelBuilder);
            ConfigureStockDataPoint<RedditMentionData>(modelBuilder);
            ConfigureStockDataPoint<FinraShortInterestData>(modelBuilder);

            // Configure ChartExchange data points
            ConfigureStockDataPoint<ChartExchangeFailureToDeliver>(modelBuilder);
            ConfigureStockDataPoint<ChartExchangeRedditMentions>(modelBuilder);
            ConfigureStockDataPoint<ChartExchangeOptionChain>(modelBuilder);
            ConfigureStockDataPoint<ChartExchangeStockSplit>(modelBuilder);
            ConfigureStockDataPoint<ChartExchangeShortInterest>(modelBuilder);
            ConfigureStockDataPoint<ChartExchangeShortVolume>(modelBuilder);
            ConfigureStockDataPoint<ChartExchangeBorrowFee>(modelBuilder);

            // Configure specific navigation properties
            modelBuilder.Entity<StockTicker>()
                .HasMany(s => s.PriceData)
                .WithOne(d => d.StockTicker)
                .HasForeignKey(d => d.StockTickerSymbol);

            modelBuilder.Entity<StockTicker>()
                .HasMany(s => s.VolumeData)
                .WithOne(d => d.StockTicker)
                .HasForeignKey(d => d.StockTickerSymbol);

            modelBuilder.Entity<StockTicker>()
                .HasMany(s => s.ShortPositionData)
                .WithOne(d => d.StockTicker)
                .HasForeignKey(d => d.StockTickerSymbol);

            modelBuilder.Entity<StockTicker>()
                .HasMany(s => s.FinraShortInterestData)
                .WithOne(d => d.StockTicker)
                .HasForeignKey(d => d.StockTickerSymbol);

            modelBuilder.Entity<StockTicker>()
                .HasMany(s => s.ChartExchangeFailureToDeliver)
                .WithOne(d => d.StockTicker)
                .HasForeignKey(d => d.StockTickerSymbol);

            modelBuilder.Entity<StockTicker>()
                .HasMany(s => s.ChartExchangeRedditMentions)
                .WithOne(d => d.StockTicker)
                .HasForeignKey(d => d.StockTickerSymbol);

            modelBuilder.Entity<StockTicker>()
                .HasMany(s => s.ChartExchangeOptionChain)
                .WithOne(d => d.StockTicker)
                .HasForeignKey(d => d.StockTickerSymbol);

            modelBuilder.Entity<StockTicker>()
                .HasMany(s => s.ChartExchangeStockSplit)
                .WithOne(d => d.StockTicker)
                .HasForeignKey(d => d.StockTickerSymbol);

            modelBuilder.Entity<StockTicker>()
                .HasMany(s => s.ChartExchangeShortInterest)
                .WithOne(d => d.StockTicker)
                .HasForeignKey(d => d.StockTickerSymbol);

            modelBuilder.Entity<StockTicker>()
                .HasMany(s => s.ChartExchangeShortVolume)
                .WithOne(d => d.StockTicker)
                .HasForeignKey(d => d.StockTickerSymbol);

            modelBuilder.Entity<StockTicker>()
                .HasMany(s => s.ChartExchangeBorrowFee)
                .WithOne(d => d.StockTicker)
                .HasForeignKey(d => d.StockTickerSymbol);
        }
    }
}

// Made with Bob
