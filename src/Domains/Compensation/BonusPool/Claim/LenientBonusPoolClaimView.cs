namespace Acme.Workforce.Compensation.BonusPool.Claim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient bonus pool claim.</summary>
public sealed record LenientBonusPoolClaimView(string Code, string Name, string JobCodeText, string Location, string Reviewer, string Segment, string EmployeeIdText, string TenureMonthsText, string StatusLabel)
{
    public static LenientBonusPoolClaimView ToLenientBonusPoolClaimView(LenientBonusPoolClaim item)
    {
        return new LenientBonusPoolClaimView(
            item.Code,
            item.Name,
            Text.Truncate(item.JobCode, 120),
            Text.Truncate(item.Location, 120),
            Text.Truncate(item.Reviewer, 40),
            item.Segment,
            Text.Truncate(item.EmployeeId, 40),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientBonusPoolClaimView> ToLenientBonusPoolClaimViews(List<LenientBonusPoolClaim> items)
    {
        var views = new List<LenientBonusPoolClaimView>();
        foreach (var item in items)
        {
            views.Add(ToLenientBonusPoolClaimView(item));
        }
        return views;
    }

    public static double SumLenientBonusPoolClaimView(LenientBonusPoolClaimView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths;
    }
}
