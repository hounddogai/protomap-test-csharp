namespace Acme.Workforce.Absence.Balance.BalanceTarget;

using System.Globalization;

/// <summary>How screens and exports show a balance mapping.</summary>
public sealed record BalanceMappingView(string Code, string Title, string Priority, string RateText, string FloorValue, string Location, string CapacityText, string SummaryText, string EmployeeId, string StatusLabel)
{
    public static BalanceMappingView ToBalanceMappingView(BalanceMapping item)
    {
        return new BalanceMappingView(
            item.Code,
            item.Title,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BalanceMappingView> ToBalanceMappingViews(List<BalanceMapping> items)
    {
        var views = new List<BalanceMappingView>();
        foreach (var item in items)
        {
            views.Add(ToBalanceMappingView(item));
        }
        return views;
    }

    public static double SumBalanceMappingView(BalanceMappingView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedRate + parsedFloorValue;
    }
}
