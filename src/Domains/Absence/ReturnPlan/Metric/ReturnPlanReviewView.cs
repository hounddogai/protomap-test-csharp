namespace Acme.Workforce.Absence.ReturnPlan.Metric;

using System.Globalization;

/// <summary>How screens and exports show a return plan review.</summary>
public sealed record ReturnPlanReviewView(string CodeText, string Name, string Reviewer, string Margin, string BatchSize, string BudgetText, string FloorValue, string GraceDaysText, string StatusLabel)
{
    public static ReturnPlanReviewView ToReturnPlanReviewView(ReturnPlanReview item)
    {
        return new ReturnPlanReviewView(
            item.Code,
            item.Name,
            item.Reviewer,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReturnPlanReviewView> ToReturnPlanReviewViews(List<ReturnPlanReview> items)
    {
        var views = new List<ReturnPlanReviewView>();
        foreach (var item in items)
        {
            views.Add(ToReturnPlanReviewView(item));
        }
        return views;
    }

    public static double SumReturnPlanReviewView(ReturnPlanReviewView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedBatchSize + parsedBudget;
    }
}
