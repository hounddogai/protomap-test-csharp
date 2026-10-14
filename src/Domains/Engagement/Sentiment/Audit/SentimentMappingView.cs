namespace Acme.Workforce.Engagement.Sentiment.Audit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a sentiment mapping.</summary>
public sealed record SentimentMappingView(string CodeText, string Title, string ContactEmail, string GraceDays, string UpdatedAt, string StatusLabel)
{
    public static SentimentMappingView ToSentimentMappingView(SentimentMapping item)
    {
        return new SentimentMappingView(
            item.Code,
            Text.Truncate(item.Title, 80),
            Text.Truncate(item.ContactEmail, 120),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SentimentMappingView> ToSentimentMappingViews(List<SentimentMapping> items)
    {
        var views = new List<SentimentMappingView>();
        foreach (var item in items)
        {
            views.Add(ToSentimentMappingView(item));
        }
        return views;
    }

    public static double SumSentimentMappingView(SentimentMappingView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + (double)parsedUpdatedAt;
    }
}
