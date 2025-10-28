namespace StockDataApi.Models.Responses;

public class FinraDataDto
{
    public FinraShortInterestDto[] ShortInterestData { get; set; } = Array.Empty<FinraShortInterestDto>();
}