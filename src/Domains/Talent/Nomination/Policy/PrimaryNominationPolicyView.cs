namespace Acme.Workforce.Talent.Nomination.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary nomination policy.</summary>
public sealed record PrimaryNominationPolicyView(string Code, string TitleText, string Ceiling, string LocationText, string BudgetText, string StatusLabel)
{
    public static PrimaryNominationPolicyView ToPrimaryNominationPolicyView(PrimaryNominationPolicy item)
    {
        return new PrimaryNominationPolicyView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryNominationPolicyView> ToPrimaryNominationPolicyViews(List<PrimaryNominationPolicy> items)
    {
        var views = new List<PrimaryNominationPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryNominationPolicyView(item));
        }
        return views;
    }

    public static double SumPrimaryNominationPolicyView(PrimaryNominationPolicyView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        return parsedCeiling + parsedBudget;
    }
}
