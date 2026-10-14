namespace Acme.Workforce.Absence.Policy.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback policy budget.</summary>
public sealed record FallbackPolicyBudgetView(string Code, string Name, string EmployeeId, string Cost, string Fte, string ContactEmail, string EndsAt, string Minimum, string Department, string ExternalRef, string Category, string StatusLabel)
{
    public static FallbackPolicyBudgetView ToFallbackPolicyBudgetView(FallbackPolicyBudget item)
    {
        return new FallbackPolicyBudgetView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.EmployeeId,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 80),
            item.ExternalRef,
            item.Category,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackPolicyBudgetView> ToFallbackPolicyBudgetViews(List<FallbackPolicyBudget> items)
    {
        var views = new List<FallbackPolicyBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackPolicyBudgetView(item));
        }
        return views;
    }

    public static double SumFallbackPolicyBudgetView(FallbackPolicyBudgetView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return parsedCost + parsedFte + (double)parsedEndsAt;
    }
}
