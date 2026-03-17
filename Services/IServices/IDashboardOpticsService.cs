using OptiControl.Models.Dtos;

namespace OptiControl.Services.IServices;

public interface IDashboardOpticsService
{
    DashboardSummaryDto GetSummary();
    List<ActivityItemDto> GetRecentActivity(int limit = 50);
    List<MonthlyIncomeDto> GetMonthlyIncome(int months = 12);
    List<TopProductDto> GetTopProducts();
}
