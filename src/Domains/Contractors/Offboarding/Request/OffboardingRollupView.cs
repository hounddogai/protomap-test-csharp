namespace Acme.Workforce.Contractors.Offboarding.Request;

using System.Globalization;

/// <summary>How screens and exports show a offboarding rollup.</summary>
public sealed record OffboardingRollupView(string Code, string Label, string Region, string VisibleText, string BudgetText, string Retries, string CreatedAt, string StartsAt, string StatusLabel)
{
    public static OffboardingRollupView ToOffboardingRollupView(OffboardingRollup item)
    {
        return new OffboardingRollupView(
            item.Code,
            item.Label,
            item.Region,
            $"{item.Visible}",
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OffboardingRollupView> ToOffboardingRollupViews(List<OffboardingRollup> items)
    {
        var views = new List<OffboardingRollupView>();
        foreach (var item in items)
        {
            views.Add(ToOffboardingRollupView(item));
        }
        return views;
    }

    public static double SumOffboardingRollupView(OffboardingRollupView view)
    {
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return parsedBudget + (double)parsedRetries + (double)parsedCreatedAt;
    }
}
