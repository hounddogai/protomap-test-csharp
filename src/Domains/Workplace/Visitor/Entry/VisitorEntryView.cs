namespace Acme.Workforce.Workplace.Visitor.Entry;

using System.Globalization;

/// <summary>How screens and exports show a visitor entry.</summary>
public sealed record VisitorEntryView(string Code, string TitleText, string ExpiresAt, string Revision, string Location, string CreatedAt, string WindowDays, string Margin, string ChannelText, string Timezone, string StatusLabel)
{
    public static VisitorEntryView ToVisitorEntryView(VisitorEntry item)
    {
        return new VisitorEntryView(
            item.Code,
            item.Title,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Location,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Channel,
            item.Timezone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VisitorEntryView> ToVisitorEntryViews(List<VisitorEntry> items)
    {
        var views = new List<VisitorEntryView>();
        foreach (var item in items)
        {
            views.Add(ToVisitorEntryView(item));
        }
        return views;
    }

    public static double SumVisitorEntryView(VisitorEntryView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedRevision + (double)parsedCreatedAt;
    }
}
