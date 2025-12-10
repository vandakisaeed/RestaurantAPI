using System;
using System.Linq;
using System.Threading.Tasks;
using RestaurantAPI.Models;
using RestaurantAPI.Dtos.Reports;

namespace RestaurantAPI.Services;

public class ReportServiceMock : IReportService
{
    private readonly IOrderService _orderService;

    public ReportServiceMock(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<SummaryReportResponseDto> GetSummaryAsync(DateOnly start, DateOnly end, string type)
    {
        if (start > end) throw new ArgumentException("Start date must be before end date.");

        var orders = await _orderService.ListAsync();
        var requestedType = string.IsNullOrWhiteSpace(type) ? null : type;

        // var ordersInRange = orders.Where(t =>
        // {
        //     var orderDate = DateOnly.FromDateTime(t.Date);
        //     bool isInDateRange = orderDate >= start && orderDate <= end;

        //     if (requestedType is null) return isInDateRange;

        //     if (string.Equals(requestedType, "InRestaurant", StringComparison.OrdinalIgnoreCase))
        //         return isInDateRange && t.Type == OrderType.InRestaurant;

        //     if (string.Equals(requestedType, "Takeout", StringComparison.OrdinalIgnoreCase))
        //         return isInDateRange && t.Type == OrderType.Takeout;

        //     return isInDateRange;
        // }).ToArray();

        var totalIncome = orders.Sum(t => t.Amount);

        return new SummaryReportResponseDto(start, end, totalIncome);
    }
}