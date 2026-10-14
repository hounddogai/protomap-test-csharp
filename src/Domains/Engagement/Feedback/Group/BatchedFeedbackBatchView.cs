namespace Acme.Workforce.Engagement.Feedback.Group;

using System.Globalization;

/// <summary>How screens and exports show a batched feedback batch.</summary>
public sealed record BatchedFeedbackBatchView(string CodeText, string Title, string Category, string Segment, string Margin, string NotesText, string EmployeeIdText, string DiscountText, string Amount, string Reviewer, string StatusLabel)
{
    public static BatchedFeedbackBatchView ToBatchedFeedbackBatchView(BatchedFeedbackBatch item)
    {
        return new BatchedFeedbackBatchView(
            item.Code,
            item.Title,
            item.Category,
            item.Segment,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Notes,
            item.EmployeeId,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Reviewer,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedFeedbackBatchView> ToBatchedFeedbackBatchViews(List<BatchedFeedbackBatch> items)
    {
        var views = new List<BatchedFeedbackBatchView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedFeedbackBatchView(item));
        }
        return views;
    }

    public static double SumBatchedFeedbackBatchView(BatchedFeedbackBatchView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedMargin + parsedDiscount + parsedAmount;
    }
}
