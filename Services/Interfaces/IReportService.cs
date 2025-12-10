using RestaurantAPI.Dtos.Reports;


namespace RestaurantAPI.Services;

public interface IReportService
{
    Task<SummaryReportResponseDto> GetSummaryAsync(DateOnly start, DateOnly end, string type);
}