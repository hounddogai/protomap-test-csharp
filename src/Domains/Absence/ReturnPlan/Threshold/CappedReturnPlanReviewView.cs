namespace Acme.Workforce.Absence.ReturnPlan.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped return plan review.</summary>
public sealed record CappedReturnPlanReviewView(string Code, string Title, string Threshold, string EmployeeIdText, string Currency, string TimezoneText, string LimitText, string ContactEmailText, string GraceDaysText, string Minimum, string EndsAt, string StatusLabel)
{
    public static CappedReturnPlanReviewView ToCappedReturnPlanReviewView(CappedReturnPlanReview item)
    {
        return new CappedReturnPlanReviewView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 120),
            Text.Truncate(item.Currency, 120),
            item.Timezone,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedReturnPlanReviewView> ToCappedReturnPlanReviewViews(List<CappedReturnPlanReview> items)
    {
        var views = new List<CappedReturnPlanReviewView>();
        foreach (var item in items)
        {
            views.Add(ToCappedReturnPlanReviewView(item));
        }
        return views;
    }

    public static double SumCappedReturnPlanReviewView(CappedReturnPlanReviewView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedLimit + (double)parsedGraceDays;
    }
}
