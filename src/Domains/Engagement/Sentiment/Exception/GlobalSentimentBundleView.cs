namespace Acme.Workforce.Engagement.Sentiment.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global sentiment bundle.</summary>
public sealed record GlobalSentimentBundleView(string Code, string Name, string SegmentText, string Timezone, string WeightText, string EndsAt, string ScoreText, string CostText, string StatusLabel)
{
    public static GlobalSentimentBundleView ToGlobalSentimentBundleView(GlobalSentimentBundle item)
    {
        return new GlobalSentimentBundleView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.Segment,
            item.Timezone,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalSentimentBundleView> ToGlobalSentimentBundleViews(List<GlobalSentimentBundle> items)
    {
        var views = new List<GlobalSentimentBundleView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalSentimentBundleView(item));
        }
        return views;
    }

    public static double SumGlobalSentimentBundleView(GlobalSentimentBundleView view)
    {
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return parsedWeight + (double)parsedEndsAt + parsedScore;
    }
}
