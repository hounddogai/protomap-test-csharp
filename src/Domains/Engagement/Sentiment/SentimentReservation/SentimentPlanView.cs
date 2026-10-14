namespace Acme.Workforce.Engagement.Sentiment.SentimentReservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a sentiment plan.</summary>
public sealed record SentimentPlanView(string Code, string LabelText, string ScoreText, string Category, string FteText, string Threshold, string OwnerId, string Attempts, string HoursText, string Minimum, string Active, string StatusLabel)
{
    public static SentimentPlanView ToSentimentPlanView(SentimentPlan item)
    {
        return new SentimentPlanView(
            item.Code,
            item.Label,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.OwnerId, 120),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SentimentPlanView> ToSentimentPlanViews(List<SentimentPlan> items)
    {
        var views = new List<SentimentPlanView>();
        foreach (var item in items)
        {
            views.Add(ToSentimentPlanView(item));
        }
        return views;
    }

    public static double SumSentimentPlanView(SentimentPlanView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return parsedScore + parsedFte + parsedThreshold;
    }
}
