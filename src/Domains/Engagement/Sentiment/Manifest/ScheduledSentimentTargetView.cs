namespace Acme.Workforce.Engagement.Sentiment.Manifest;

using System.Globalization;

/// <summary>How screens and exports show a scheduled sentiment target.</summary>
public sealed record ScheduledSentimentTargetView(string CodeText, string NameText, string UpdatedAt, string GraceDaysText, string Limit, string Source, string StatusLabel)
{
    public static ScheduledSentimentTargetView ToScheduledSentimentTargetView(ScheduledSentimentTarget item)
    {
        return new ScheduledSentimentTargetView(
            item.Code,
            item.Name,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Source,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledSentimentTargetView> ToScheduledSentimentTargetViews(List<ScheduledSentimentTarget> items)
    {
        var views = new List<ScheduledSentimentTargetView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledSentimentTargetView(item));
        }
        return views;
    }

    public static double SumScheduledSentimentTargetView(ScheduledSentimentTargetView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedGraceDays + (double)parsedLimit;
    }
}
