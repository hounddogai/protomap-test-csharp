namespace Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

using System.Globalization;

/// <summary>How screens and exports show a access badge preference.</summary>
public sealed record AccessBadgePreferenceView(string Code, string Title, string SummaryText, string FloorValue, string Ceiling, string ThresholdText, string StatusLabel)
{
    public static AccessBadgePreferenceView ToAccessBadgePreferenceView(AccessBadgePreference item)
    {
        return new AccessBadgePreferenceView(
            item.Code,
            item.Title,
            item.Summary,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccessBadgePreferenceView> ToAccessBadgePreferenceViews(List<AccessBadgePreference> items)
    {
        var views = new List<AccessBadgePreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToAccessBadgePreferenceView(item));
        }
        return views;
    }

    public static double SumAccessBadgePreferenceView(AccessBadgePreferenceView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedCeiling + parsedThreshold;
    }
}
