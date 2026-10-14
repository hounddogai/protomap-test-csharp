namespace Acme.Workforce.Engagement.Feedback.Score;

using System.Globalization;

/// <summary>How screens and exports show a batched feedback score.</summary>
public sealed record BatchedFeedbackScoreView(string Code, string TitleText, string ReviewerText, string CostText, string CurrencyText, string Rate, string Ceiling, string Version, string ExternalRef, string WindowDays, string Hours, string StatusLabel)
{
    public static BatchedFeedbackScoreView ToBatchedFeedbackScoreView(BatchedFeedbackScore item)
    {
        return new BatchedFeedbackScoreView(
            item.Code,
            item.Title,
            item.Reviewer,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedFeedbackScoreView> ToBatchedFeedbackScoreViews(List<BatchedFeedbackScore> items)
    {
        var views = new List<BatchedFeedbackScoreView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedFeedbackScoreView(item));
        }
        return views;
    }

    public static double SumBatchedFeedbackScoreView(BatchedFeedbackScoreView view)
    {
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return parsedCost + parsedRate + parsedCeiling;
    }
}
