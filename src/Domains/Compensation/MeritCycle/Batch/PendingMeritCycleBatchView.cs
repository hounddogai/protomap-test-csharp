namespace Acme.Workforce.Compensation.MeritCycle.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending merit cycle batch.</summary>
public sealed record PendingMeritCycleBatchView(string Code, string TitleText, string PinnedText, string ScoreText, string Description, string UpdatedAtText, string StartsAtText, string Region, string StatusLabel)
{
    public static PendingMeritCycleBatchView ToPendingMeritCycleBatchView(PendingMeritCycleBatch item)
    {
        return new PendingMeritCycleBatchView(
            item.Code,
            Text.Truncate(item.Title, 80),
            $"{item.Pinned}",
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 64),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Region,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingMeritCycleBatchView> ToPendingMeritCycleBatchViews(List<PendingMeritCycleBatch> items)
    {
        var views = new List<PendingMeritCycleBatchView>();
        foreach (var item in items)
        {
            views.Add(ToPendingMeritCycleBatchView(item));
        }
        return views;
    }

    public static double SumPendingMeritCycleBatchView(PendingMeritCycleBatchView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedUpdatedAt + (double)parsedStartsAt;
    }
}
