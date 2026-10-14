namespace Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a sentiment draft.</summary>
public sealed record SentimentDraftView(string CodeText, string TitleText, string Location, string ChecksumText, string Capacity, string Badge, string Reviewer, string StatusLabel)
{
    public static SentimentDraftView ToSentimentDraftView(SentimentDraft item)
    {
        return new SentimentDraftView(
            item.Code,
            item.Title,
            Text.Truncate(item.Location, 80),
            item.Checksum,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            item.Reviewer,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SentimentDraftView> ToSentimentDraftViews(List<SentimentDraft> items)
    {
        var views = new List<SentimentDraftView>();
        foreach (var item in items)
        {
            views.Add(ToSentimentDraftView(item));
        }
        return views;
    }

    public static double SumSentimentDraftView(SentimentDraftView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return (double)parsedCapacity;
    }
}
