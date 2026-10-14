namespace Acme.Workforce.Compensation.EquityGrant.Exception;

using System.Globalization;

/// <summary>How screens and exports show a weighted equity grant window.</summary>
public sealed record WeightedEquityGrantWindowView(string Code, string Name, string LeaveDays, string CapacityText, string Country, string GraceDays, string ManagerId, string Badge, string Minimum, string StatusLabel)
{
    public static WeightedEquityGrantWindowView ToWeightedEquityGrantWindowView(WeightedEquityGrantWindow item)
    {
        return new WeightedEquityGrantWindowView(
            item.Code,
            item.Name,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Country,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            item.Badge,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedEquityGrantWindowView> ToWeightedEquityGrantWindowViews(List<WeightedEquityGrantWindow> items)
    {
        var views = new List<WeightedEquityGrantWindowView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedEquityGrantWindowView(item));
        }
        return views;
    }

    public static double SumWeightedEquityGrantWindowView(WeightedEquityGrantWindowView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedCapacity + (double)parsedGraceDays;
    }
}
