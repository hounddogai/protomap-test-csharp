namespace Acme.Workforce.Engagement.Sentiment.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a sentiment claim.</summary>
public sealed record SentimentClaimView(string Code, string TitleText, string Label, string ActiveText, string CategoryText, string Segment, string Score, string StatusLabel)
{
    public static SentimentClaimView ToSentimentClaimView(SentimentClaim item)
    {
        return new SentimentClaimView(
            item.Code,
            item.Title,
            item.Label,
            $"{item.Active}",
            item.Category,
            Text.Truncate(item.Segment, 120),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SentimentClaimView> ToSentimentClaimViews(List<SentimentClaim> items)
    {
        var views = new List<SentimentClaimView>();
        foreach (var item in items)
        {
            views.Add(ToSentimentClaimView(item));
        }
        return views;
    }

    public static double SumSentimentClaimView(SentimentClaimView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
