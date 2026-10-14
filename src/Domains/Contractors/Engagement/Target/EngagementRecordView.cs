namespace Acme.Workforce.Contractors.Engagement.Target;

using System.Globalization;

/// <summary>How screens and exports show a engagement record.</summary>
public sealed record EngagementRecordView(string CodeText, string TitleText, string ExternalRefText, string FteText, string Summary, string Channel, string Segment, string DurationMinutesText, string StatusLabel)
{
    public static EngagementRecordView ToEngagementRecordView(EngagementRecord item)
    {
        return new EngagementRecordView(
            item.Code,
            item.Title,
            item.ExternalRef,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            item.Channel,
            item.Segment,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EngagementRecordView> ToEngagementRecordViews(List<EngagementRecord> items)
    {
        var views = new List<EngagementRecordView>();
        foreach (var item in items)
        {
            views.Add(ToEngagementRecordView(item));
        }
        return views;
    }

    public static double SumEngagementRecordView(EngagementRecordView view)
    {
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedDurationMinutes;
    }
}
