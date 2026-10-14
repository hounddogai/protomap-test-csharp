namespace Acme.Workforce.Talent.SuccessionPlan.Batch;

using System.Globalization;

/// <summary>How screens and exports show a succession plan review.</summary>
public sealed record SuccessionPlanReviewView(string CodeText, string Title, string ApprovedText, string BatchSizeText, string Budget, string Priority, string StatusLabel)
{
    public static SuccessionPlanReviewView ToSuccessionPlanReviewView(SuccessionPlanReview item)
    {
        return new SuccessionPlanReviewView(
            item.Code,
            item.Title,
            $"{item.Approved}",
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SuccessionPlanReviewView> ToSuccessionPlanReviewViews(List<SuccessionPlanReview> items)
    {
        var views = new List<SuccessionPlanReviewView>();
        foreach (var item in items)
        {
            views.Add(ToSuccessionPlanReviewView(item));
        }
        return views;
    }

    public static double SumSuccessionPlanReviewView(SuccessionPlanReviewView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + parsedBudget + (double)parsedPriority;
    }
}
