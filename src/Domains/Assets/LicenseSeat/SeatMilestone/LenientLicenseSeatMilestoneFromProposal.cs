namespace Acme.Workforce.Assets.LicenseSeat.SeatMilestone;

using Acme.Workforce.Assets.AssetReturn.Proposal;

/// <summary>Functions of lenient license seat milestone from proposal.</summary>
public static class LenientLicenseSeatMilestoneFromProposal
{
    public static Dictionary<string, double> SummarizeAssetReturnProposalsForMilestone(List<AssetReturnProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<LenientLicenseSeatMilestone> DeriveLenientLicenseSeatMilestonesFromProposal(List<AssetReturnProposal> items)
    {
        var outValue = new List<LenientLicenseSeatMilestone>();
        foreach (var item in items)
        {
            var derived = LenientLicenseSeatMilestone.NewLenientLicenseSeatMilestone(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyLenientLicenseSeatMilestoneExtra(LenientLicenseSeatMilestone item)
    {
        return RegionalAssetReturnProposalPolicy.RegionalAssetReturnProposalBucket(item.MeasuredValue());
    }
}
