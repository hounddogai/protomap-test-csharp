namespace Acme.Workforce.Assets.Refresh.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive refresh checkpoint.</summary>
public sealed record AdaptiveRefreshCheckpointView(string CodeText, string Name, string ContactEmailText, string Score, string Grade, string Rating, string TitleText, string PinnedText, string StatusLabel)
{
    public static AdaptiveRefreshCheckpointView ToAdaptiveRefreshCheckpointView(AdaptiveRefreshCheckpoint item)
    {
        return new AdaptiveRefreshCheckpointView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.ContactEmail,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 80),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Title,
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveRefreshCheckpointView> ToAdaptiveRefreshCheckpointViews(List<AdaptiveRefreshCheckpoint> items)
    {
        var views = new List<AdaptiveRefreshCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveRefreshCheckpointView(item));
        }
        return views;
    }

    public static double SumAdaptiveRefreshCheckpointView(AdaptiveRefreshCheckpointView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return parsedScore + parsedRating;
    }
}
