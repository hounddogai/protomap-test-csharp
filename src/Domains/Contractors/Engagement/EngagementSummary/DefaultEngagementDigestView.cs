namespace Acme.Workforce.Contractors.Engagement.EngagementSummary;

using System.Globalization;

/// <summary>How screens and exports show a default engagement digest.</summary>
public sealed record DefaultEngagementDigestView(string CodeText, string Label, string Name, string Archived, string BatchSizeText, string StatusLabel)
{
    public static DefaultEngagementDigestView ToDefaultEngagementDigestView(DefaultEngagementDigest item)
    {
        return new DefaultEngagementDigestView(
            item.Code,
            item.Label,
            item.Name,
            $"{item.Archived}",
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultEngagementDigestView> ToDefaultEngagementDigestViews(List<DefaultEngagementDigest> items)
    {
        var views = new List<DefaultEngagementDigestView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultEngagementDigestView(item));
        }
        return views;
    }

    public static double SumDefaultEngagementDigestView(DefaultEngagementDigestView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize;
    }
}
