namespace Acme.Workforce.Engagement.Feedback.Rule;

using System.Globalization;

/// <summary>How screens and exports show a batched feedback baseline.</summary>
public sealed record BatchedFeedbackBaselineView(string Code, string TitleText, string Segment, string GraceDaysText, string Department, string EndsAtText, string Currency, string EmployeeIdText, string JobCode, string StatusLabel)
{
    public static BatchedFeedbackBaselineView ToBatchedFeedbackBaselineView(BatchedFeedbackBaseline item)
    {
        return new BatchedFeedbackBaselineView(
            item.Code,
            item.Title,
            item.Segment,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Currency,
            item.EmployeeId,
            item.JobCode,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedFeedbackBaselineView> ToBatchedFeedbackBaselineViews(List<BatchedFeedbackBaseline> items)
    {
        var views = new List<BatchedFeedbackBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedFeedbackBaselineView(item));
        }
        return views;
    }

    public static double SumBatchedFeedbackBaselineView(BatchedFeedbackBaselineView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + (double)parsedEndsAt;
    }
}
