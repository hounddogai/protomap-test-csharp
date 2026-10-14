namespace Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a sentiment score.</summary>
public sealed record SentimentScoreView(string Code, string Name, string Timezone, string RatioText, string PinnedText, string BadgeText, string JobCode, string Revision, string Priority, string StatusLabel)
{
    public static SentimentScoreView ToSentimentScoreView(SentimentScore item)
    {
        return new SentimentScoreView(
            item.Code,
            item.Name,
            Text.Truncate(item.Timezone, 64),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Badge,
            item.JobCode,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SentimentScoreView> ToSentimentScoreViews(List<SentimentScore> items)
    {
        var views = new List<SentimentScoreView>();
        foreach (var item in items)
        {
            views.Add(ToSentimentScoreView(item));
        }
        return views;
    }

    public static double SumSentimentScoreView(SentimentScoreView view)
    {
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedRevision + (double)parsedPriority;
    }
}
