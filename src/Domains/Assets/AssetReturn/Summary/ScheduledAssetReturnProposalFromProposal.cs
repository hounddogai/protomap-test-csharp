namespace Acme.Workforce.Assets.AssetReturn.Summary;

using Acme.Workforce.Assets.Warranty.Reservation;

/// <summary>Functions of scheduled asset return proposal from proposal.</summary>
public static class ScheduledAssetReturnProposalFromProposal
{
    public static Dictionary<string, double> SummarizeRegionalWarrantyProposalsForProposal(List<RegionalWarrantyProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<ScheduledAssetReturnProposal> DeriveScheduledAssetReturnProposalsFromProposal(List<RegionalWarrantyProposal> items)
    {
        var outValue = new List<ScheduledAssetReturnProposal>();
        foreach (var item in items)
        {
            var derived = ScheduledAssetReturnProposal.NewScheduledAssetReturnProposal(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectScheduledAssetReturnProposal(ScheduledAssetReturnProposal item)
    {
        return WarrantyBaselinePolicy.WarrantyBaselineBucket(item.MeasuredValue());
    }
}
