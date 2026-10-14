namespace Acme.Workforce.Engagement.Sentiment.Variance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a sentiment rollup.</summary>
public sealed record SentimentRollupView(string Code, string Title, string BadgeText, string Channel, string Timezone, string AmountText, string StatusLabel)
{
    public static SentimentRollupView ToSentimentRollupView(SentimentRollup item)
    {
        return new SentimentRollupView(
            item.Code,
            item.Title,
            item.Badge,
            Text.Truncate(item.Channel, 80),
            Text.Truncate(item.Timezone, 120),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SentimentRollupView> ToSentimentRollupViews(List<SentimentRollup> items)
    {
        var views = new List<SentimentRollupView>();
        foreach (var item in items)
        {
            views.Add(ToSentimentRollupView(item));
        }
        return views;
    }

    public static double SumSentimentRollupView(SentimentRollupView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
