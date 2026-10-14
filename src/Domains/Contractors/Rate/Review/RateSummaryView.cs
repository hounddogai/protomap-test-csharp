namespace Acme.Workforce.Contractors.Rate.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rate summary.</summary>
public sealed record RateSummaryView(string CodeText, string Name, string FloorValue, string PhoneText, string Title, string ExternalRefText, string LocationText, string TimezoneText, string CostText, string StatusLabel)
{
    public static RateSummaryView ToRateSummaryView(RateSummary item)
    {
        return new RateSummaryView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            item.Title,
            Text.Truncate(item.ExternalRef, 64),
            item.Location,
            Text.Truncate(item.Timezone, 120),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RateSummaryView> ToRateSummaryViews(List<RateSummary> items)
    {
        var views = new List<RateSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToRateSummaryView(item));
        }
        return views;
    }

    public static double SumRateSummaryView(RateSummaryView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedCost;
    }
}
