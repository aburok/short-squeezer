namespace StockDataApi.Models.Responses;

public class ChartExchangeOptionChainDataDto
{
    public DateTimeOffset Date { get; set; }
    public string ExpirationDate { get; set; } = string.Empty;
    public decimal StrikePrice { get; set; }
    public string OptionType { get; set; } = string.Empty;
    public long Volume { get; set; }
    public long OpenInterest { get; set; }
    public decimal? Bid { get; set; }
    public decimal? Ask { get; set; }
    public decimal? LastPrice { get; set; }
    public decimal? ImpliedVolatility { get; set; }
    public decimal? Delta { get; set; }
    public decimal? Gamma { get; set; }
    public decimal? Theta { get; set; }
    public decimal? Vega { get; set; }
}