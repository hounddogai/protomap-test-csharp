namespace Acme.Workforce.Engagement.Sentiment.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a sentiment segment.</summary>
public sealed record SentimentSegmentView(string Code, string Name, string ArchivedText, string Team, string Phone, string Amount, string StatusLabel)
{
    public static SentimentSegmentView ToSentimentSegmentView(SentimentSegment item)
    {
        return new SentimentSegmentView(
            item.Code,
            item.Name,
            $"{item.Archived}",
            item.Team,
            Text.Truncate(item.Phone, 64),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SentimentSegmentView> ToSentimentSegmentViews(List<SentimentSegment> items)
    {
        var views = new List<SentimentSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToSentimentSegmentView(item));
        }
        return views;
    }

    public static double SumSentimentSegmentView(SentimentSegmentView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
