namespace Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal sentiment draft.</summary>
public sealed record SeasonalSentimentDraftView(string Code, string Label, string ExternalRef, string Checksum, string VisibleText, string QuantityText, string StatusLabel)
{
    public static SeasonalSentimentDraftView ToSeasonalSentimentDraftView(SeasonalSentimentDraft item)
    {
        return new SeasonalSentimentDraftView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.ExternalRef,
            item.Checksum,
            $"{item.Visible}",
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalSentimentDraftView> ToSeasonalSentimentDraftViews(List<SeasonalSentimentDraft> items)
    {
        var views = new List<SeasonalSentimentDraftView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalSentimentDraftView(item));
        }
        return views;
    }

    public static double SumSeasonalSentimentDraftView(SeasonalSentimentDraftView view)
    {
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
