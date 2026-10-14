namespace Acme.Workforce.Assets.LicenseSeat.Proposal;

using Acme.Workforce.Assets.Warranty.Milestone;

/// <summary>Functions of license seat proposal from target.</summary>
public static class LicenseSeatProposalFromTarget
{
    public static Dictionary<string, double> SummarizeSeasonalWarrantyTargetsForProposal(List<SeasonalWarrantyTarget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<LicenseSeatProposal> DeriveLicenseSeatProposalsFromTarget(List<SeasonalWarrantyTarget> items)
    {
        var outValue = new List<LicenseSeatProposal>();
        foreach (var item in items)
        {
            var derived = LicenseSeatProposal.NewLicenseSeatProposal(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyLicenseSeatProposal(LicenseSeatProposal item)
    {
        return SeasonalWarrantyTargetPolicy.SeasonalWarrantyTargetBucket(item.MeasuredValue());
    }
}
