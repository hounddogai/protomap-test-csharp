namespace Acme.Workforce.Payrollops.Deduction.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a deduction estimate.</summary>
public sealed record DeductionEstimateView(string Code, string TitleText, string BadgeText, string Timezone, string ExpiresAt, string Attempts, string Summary, string Locale, string ExternalRefText, string BudgetText, string LeaveDaysText, string StatusLabel)
{
    public static DeductionEstimateView ToDeductionEstimateView(DeductionEstimate item)
    {
        return new DeductionEstimateView(
            item.Code,
            item.Title,
            Text.Truncate(item.Badge, 80),
            Text.Truncate(item.Timezone, 80),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Summary, 80),
            item.Locale,
            Text.Truncate(item.ExternalRef, 64),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeductionEstimateView> ToDeductionEstimateViews(List<DeductionEstimate> items)
    {
        var views = new List<DeductionEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToDeductionEstimateView(item));
        }
        return views;
    }

    public static double SumDeductionEstimateView(DeductionEstimateView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedAttempts + parsedBudget;
    }
}
