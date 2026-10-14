namespace Acme.Workforce.Contractors.Rate.Snapshot;

using System.Globalization;

/// <summary>How screens and exports show a seasonal rate snapshot.</summary>
public sealed record SeasonalRateSnapshotView(string Code, string Label, string Attempts, string UpdatedAt, string Visible, string ChannelText, string Hours, string StatusLabel)
{
    public static SeasonalRateSnapshotView ToSeasonalRateSnapshotView(SeasonalRateSnapshot item)
    {
        return new SeasonalRateSnapshotView(
            item.Code,
            item.Label,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Channel,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalRateSnapshotView> ToSeasonalRateSnapshotViews(List<SeasonalRateSnapshot> items)
    {
        var views = new List<SeasonalRateSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalRateSnapshotView(item));
        }
        return views;
    }

    public static double SumSeasonalRateSnapshotView(SeasonalRateSnapshotView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedUpdatedAt + parsedHours;
    }
}
