namespace RestaurantAPI.Dtos.Reports;

public record SummaryReportResponseDto(
  DateOnly StartDate,
  DateOnly EndDate,
  decimal TotalIncome
  );