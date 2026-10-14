namespace Acme.Workforce.Absence.LeaveRequest.RequestBudget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal leave request budget.</summary>
public sealed record SeasonalLeaveRequestBudgetView(string Code, string Name, string Quantity, string BatchSize, string CostCenter, string FloorValueText, string WindowDays, string CategoryText, string ApprovedText, string StatusLabel)
{
    public static SeasonalLeaveRequestBudgetView ToSeasonalLeaveRequestBudgetView(SeasonalLeaveRequestBudget item)
    {
        return new SeasonalLeaveRequestBudgetView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.CostCenter,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Category,
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalLeaveRequestBudgetView> ToSeasonalLeaveRequestBudgetViews(List<SeasonalLeaveRequestBudget> items)
    {
        var views = new List<SeasonalLeaveRequestBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalLeaveRequestBudgetView(item));
        }
        return views;
    }

    public static double SumSeasonalLeaveRequestBudgetView(SeasonalLeaveRequestBudgetView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + (double)parsedBatchSize + parsedFloorValue;
    }
}
