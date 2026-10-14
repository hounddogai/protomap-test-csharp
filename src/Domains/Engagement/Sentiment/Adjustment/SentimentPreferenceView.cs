namespace Acme.Workforce.Engagement.Sentiment.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a sentiment preference.</summary>
public sealed record SentimentPreferenceView(string Code, string Title, string Channel, string Quantity, string ExternalRefText, string TeamText, string StatusLabel)
{
    public static SentimentPreferenceView ToSentimentPreferenceView(SentimentPreference item)
    {
        return new SentimentPreferenceView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.Channel,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Team,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SentimentPreferenceView> ToSentimentPreferenceViews(List<SentimentPreference> items)
    {
        var views = new List<SentimentPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToSentimentPreferenceView(item));
        }
        return views;
    }

    public static double SumSentimentPreferenceView(SentimentPreferenceView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
