namespace Acme.Workforce.Contractors.Engagement.Tier;

using System.Globalization;

/// <summary>How screens and exports show a engagement summary.</summary>
public sealed record EngagementSummaryView(string Code, string Label, string Badge, string Rating, string EndsAtText, string Name, string TitleText, string ContactEmail, string Checksum, string Fte, string StatusLabel)
{
    public static EngagementSummaryView ToEngagementSummaryView(EngagementSummary item)
    {
        return new EngagementSummaryView(
            item.Code,
            item.Label,
            item.Badge,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Name,
            item.Title,
            item.ContactEmail,
            item.Checksum,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EngagementSummaryView> ToEngagementSummaryViews(List<EngagementSummary> items)
    {
        var views = new List<EngagementSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToEngagementSummaryView(item));
        }
        return views;
    }

    public static double SumEngagementSummaryView(EngagementSummaryView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return parsedRating + (double)parsedEndsAt + parsedFte;
    }
}
