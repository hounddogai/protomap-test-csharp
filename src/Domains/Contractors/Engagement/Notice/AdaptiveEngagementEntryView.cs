namespace Acme.Workforce.Contractors.Engagement.Notice;

using System.Globalization;

/// <summary>How screens and exports show a adaptive engagement entry.</summary>
public sealed record AdaptiveEngagementEntryView(string CodeText, string LabelText, string Source, string RateText, string Ceiling, string Revision, string StatusLabel)
{
    public static AdaptiveEngagementEntryView ToAdaptiveEngagementEntryView(AdaptiveEngagementEntry item)
    {
        return new AdaptiveEngagementEntryView(
            item.Code,
            item.Label,
            item.Source,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveEngagementEntryView> ToAdaptiveEngagementEntryViews(List<AdaptiveEngagementEntry> items)
    {
        var views = new List<AdaptiveEngagementEntryView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveEngagementEntryView(item));
        }
        return views;
    }

    public static double SumAdaptiveEngagementEntryView(AdaptiveEngagementEntryView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return parsedRate + parsedCeiling + (double)parsedRevision;
    }
}
