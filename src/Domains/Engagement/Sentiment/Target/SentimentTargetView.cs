namespace Acme.Workforce.Engagement.Sentiment.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a sentiment target.</summary>
public sealed record SentimentTargetView(string Code, string TitleText, string Description, string Checksum, string JobCode, string Team, string Summary, string ContactEmail, string Quantity, string StatusLabel)
{
    public static SentimentTargetView ToSentimentTargetView(SentimentTarget item)
    {
        return new SentimentTargetView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.Description,
            item.Checksum,
            item.JobCode,
            Text.Truncate(item.Team, 120),
            Text.Truncate(item.Summary, 40),
            item.ContactEmail,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SentimentTargetView> ToSentimentTargetViews(List<SentimentTarget> items)
    {
        var views = new List<SentimentTargetView>();
        foreach (var item in items)
        {
            views.Add(ToSentimentTargetView(item));
        }
        return views;
    }

    public static double SumSentimentTargetView(SentimentTargetView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
