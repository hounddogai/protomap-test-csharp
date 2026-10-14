namespace Acme.Workforce.Compliance.TrainingDue.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a training due batch.</summary>
public sealed record TrainingDueBatchView(string CodeText, string NameText, string Locale, string Segment, string Rating, string Attempts, string ScoreText, string Margin, string Source, string StatusLabel)
{
    public static TrainingDueBatchView ToTrainingDueBatchView(TrainingDueBatch item)
    {
        return new TrainingDueBatchView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.Locale,
            Text.Truncate(item.Segment, 80),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Source,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TrainingDueBatchView> ToTrainingDueBatchViews(List<TrainingDueBatch> items)
    {
        var views = new List<TrainingDueBatchView>();
        foreach (var item in items)
        {
            views.Add(ToTrainingDueBatchView(item));
        }
        return views;
    }

    public static double SumTrainingDueBatchView(TrainingDueBatchView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return parsedRating + (double)parsedAttempts + parsedScore;
    }
}
