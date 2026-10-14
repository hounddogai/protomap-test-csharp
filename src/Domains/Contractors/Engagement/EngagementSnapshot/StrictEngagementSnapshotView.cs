namespace Acme.Workforce.Contractors.Engagement.EngagementSnapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict engagement snapshot.</summary>
public sealed record StrictEngagementSnapshotView(string Code, string TitleText, string Notes, string Currency, string Country, string BatchSize, string Score, string StatusLabel)
{
    public static StrictEngagementSnapshotView ToStrictEngagementSnapshotView(StrictEngagementSnapshot item)
    {
        return new StrictEngagementSnapshotView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Title, 40),
            item.Notes,
            item.Currency,
            Text.Truncate(item.Country, 80),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictEngagementSnapshotView> ToStrictEngagementSnapshotViews(List<StrictEngagementSnapshot> items)
    {
        var views = new List<StrictEngagementSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToStrictEngagementSnapshotView(item));
        }
        return views;
    }

    public static double SumStrictEngagementSnapshotView(StrictEngagementSnapshotView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + parsedScore;
    }
}
