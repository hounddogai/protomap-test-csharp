namespace Acme.Workforce.Engagement.Sentiment.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback sentiment score.</summary>
public sealed record FallbackSentimentScoreView(string Code, string Label, string NameText, string Country, string Locale, string CostCenterText, string VersionText, string LeaveDays, string ExpiresAt, string StatusLabel)
{
    public static FallbackSentimentScoreView ToFallbackSentimentScoreView(FallbackSentimentScore item)
    {
        return new FallbackSentimentScoreView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Label, 80),
            item.Name,
            item.Country,
            item.Locale,
            item.CostCenter,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackSentimentScoreView> ToFallbackSentimentScoreViews(List<FallbackSentimentScore> items)
    {
        var views = new List<FallbackSentimentScoreView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackSentimentScoreView(item));
        }
        return views;
    }

    public static double SumFallbackSentimentScoreView(FallbackSentimentScoreView view)
    {
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedLeaveDays + (double)parsedExpiresAt;
    }
}
