namespace Acme.Workforce.Compliance.Attestation.AttestationRule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default attestation rule.</summary>
public sealed record DefaultAttestationRuleView(string Code, string Name, string Category, string Reviewer, string ExpiresAt, string BudgetText, string SalaryBand, string Currency, string StatusLabel)
{
    public static DefaultAttestationRuleView ToDefaultAttestationRuleView(DefaultAttestationRule item)
    {
        return new DefaultAttestationRuleView(
            item.Code,
            item.Name,
            Text.Truncate(item.Category, 80),
            item.Reviewer,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.Currency,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultAttestationRuleView> ToDefaultAttestationRuleViews(List<DefaultAttestationRule> items)
    {
        var views = new List<DefaultAttestationRuleView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultAttestationRuleView(item));
        }
        return views;
    }

    public static double SumDefaultAttestationRuleView(DefaultAttestationRuleView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + parsedBudget;
    }
}
