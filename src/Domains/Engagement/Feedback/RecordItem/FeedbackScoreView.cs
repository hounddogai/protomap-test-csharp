namespace Acme.Workforce.Engagement.Feedback.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a feedback score.</summary>
public sealed record FeedbackScoreView(string Code, string Name, string SourceText, string ExpiresAt, string Summary, string Quantity, string Budget, string StatusLabel)
{
    public static FeedbackScoreView ToFeedbackScoreView(FeedbackScore item)
    {
        return new FeedbackScoreView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Source,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Summary, 120),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FeedbackScoreView> ToFeedbackScoreViews(List<FeedbackScore> items)
    {
        var views = new List<FeedbackScoreView>();
        foreach (var item in items)
        {
            views.Add(ToFeedbackScoreView(item));
        }
        return views;
    }

    public static double SumFeedbackScoreView(FeedbackScoreView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedQuantity + parsedBudget;
    }
}
