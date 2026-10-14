namespace Acme.Workforce.Talent.Nomination.NominationPolicy;

using System.Globalization;

/// <summary>How screens and exports show a regional nomination policy.</summary>
public sealed record RegionalNominationPolicyView(string Code, string Name, string Fte, string LeaveDays, string Checksum, string BudgetText, string DiscountText, string SalaryBand, string Phone, string CategoryText, string ContactEmail, string StatusLabel)
{
    public static RegionalNominationPolicyView ToRegionalNominationPolicyView(RegionalNominationPolicy item)
    {
        return new RegionalNominationPolicyView(
            item.Code,
            item.Name,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.Phone,
            item.Category,
            item.ContactEmail,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalNominationPolicyView> ToRegionalNominationPolicyViews(List<RegionalNominationPolicy> items)
    {
        var views = new List<RegionalNominationPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalNominationPolicyView(item));
        }
        return views;
    }

    public static double SumRegionalNominationPolicyView(RegionalNominationPolicyView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedLeaveDays + parsedBudget;
    }
}
