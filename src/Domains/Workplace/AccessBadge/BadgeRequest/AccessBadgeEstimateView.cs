namespace Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

using System.Globalization;

/// <summary>How screens and exports show a access badge estimate.</summary>
public sealed record AccessBadgeEstimateView(string Code, string Title, string FloorValueText, string Margin, string CeilingText, string Cost, string StatusLabel)
{
    public static AccessBadgeEstimateView ToAccessBadgeEstimateView(AccessBadgeEstimate item)
    {
        return new AccessBadgeEstimateView(
            item.Code,
            item.Title,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccessBadgeEstimateView> ToAccessBadgeEstimateViews(List<AccessBadgeEstimate> items)
    {
        var views = new List<AccessBadgeEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToAccessBadgeEstimateView(item));
        }
        return views;
    }

    public static double SumAccessBadgeEstimateView(AccessBadgeEstimateView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedMargin + parsedCeiling;
    }
}
