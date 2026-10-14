namespace Acme.Workforce.Engagement.Sentiment.Audit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a sentiment schedule.</summary>
public sealed record SentimentScheduleView(string Code, string Name, string FloorValue, string ExternalRef, string Attempts, string Description, string Notes, string Threshold, string OwnerId, string StatusLabel)
{
    public static SentimentScheduleView ToSentimentScheduleView(SentimentSchedule item)
    {
        return new SentimentScheduleView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Description,
            item.Notes,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.OwnerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SentimentScheduleView> ToSentimentScheduleViews(List<SentimentSchedule> items)
    {
        var views = new List<SentimentScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToSentimentScheduleView(item));
        }
        return views;
    }

    public static double SumSentimentScheduleView(SentimentScheduleView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedAttempts + parsedThreshold;
    }
}
