namespace Acme.Workforce.Engagement.Sentiment.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a sentiment cycle.</summary>
public sealed record SentimentCycleView(string Code, string Name, string Reviewer, string Notes, string Priority, string Fte, string ScoreText, string Amount, string StatusLabel)
{
    public static SentimentCycleView ToSentimentCycleView(SentimentCycle item)
    {
        return new SentimentCycleView(
            item.Code,
            item.Name,
            item.Reviewer,
            Text.Truncate(item.Notes, 120),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SentimentCycleView> ToSentimentCycleViews(List<SentimentCycle> items)
    {
        var views = new List<SentimentCycleView>();
        foreach (var item in items)
        {
            views.Add(ToSentimentCycleView(item));
        }
        return views;
    }

    public static double SumSentimentCycleView(SentimentCycleView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedFte + parsedScore;
    }
}
