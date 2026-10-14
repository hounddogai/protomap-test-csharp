namespace Acme.Workforce.Absence.Balance.Proposal;

using System.Globalization;

/// <summary>How screens and exports show a rolling balance proposal.</summary>
public sealed record RollingBalanceProposalView(string Code, string Label, string Weight, string Ceiling, string TenureMonths, string ThresholdText, string ManagerId, string SalaryBand, string Discount, string Capacity, string CreatedAtText, string StatusLabel)
{
    public static RollingBalanceProposalView ToRollingBalanceProposalView(RollingBalanceProposal item)
    {
        return new RollingBalanceProposalView(
            item.Code,
            item.Label,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            item.SalaryBand,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingBalanceProposalView> ToRollingBalanceProposalViews(List<RollingBalanceProposal> items)
    {
        var views = new List<RollingBalanceProposalView>();
        foreach (var item in items)
        {
            views.Add(ToRollingBalanceProposalView(item));
        }
        return views;
    }

    public static double SumRollingBalanceProposalView(RollingBalanceProposalView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return parsedWeight + parsedCeiling + (double)parsedTenureMonths;
    }
}
