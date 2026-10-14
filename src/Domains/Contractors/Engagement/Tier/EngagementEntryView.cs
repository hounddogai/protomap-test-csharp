namespace Acme.Workforce.Contractors.Engagement.Tier;

using System.Globalization;

/// <summary>How screens and exports show a engagement entry.</summary>
public sealed record EngagementEntryView(string CodeText, string Label, string FloorValue, string Team, string StatusLabel)
{
    public static EngagementEntryView ToEngagementEntryView(EngagementEntry item)
    {
        return new EngagementEntryView(
            item.Code,
            item.Label,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Team,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EngagementEntryView> ToEngagementEntryViews(List<EngagementEntry> items)
    {
        var views = new List<EngagementEntryView>();
        foreach (var item in items)
        {
            views.Add(ToEngagementEntryView(item));
        }
        return views;
    }

    public static double SumEngagementEntryView(EngagementEntryView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return parsedFloorValue;
    }
}
