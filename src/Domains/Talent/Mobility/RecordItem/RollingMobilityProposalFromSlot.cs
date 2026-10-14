namespace Acme.Workforce.Talent.Mobility.RecordItem;

using Acme.Workforce.Talent.CareerPath.PathSlot;

/// <summary>Functions of rolling mobility proposal from slot.</summary>
public static class RollingMobilityProposalFromSlot
{
    public static Dictionary<string, double> SummarizeAdaptiveCareerPathSlotsForProposal(List<AdaptiveCareerPathSlot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<RollingMobilityProposal> DeriveRollingMobilityProposalsFromSlot(List<AdaptiveCareerPathSlot> items)
    {
        var outValue = new List<RollingMobilityProposal>();
        foreach (var item in items)
        {
            var derived = RollingMobilityProposal.NewRollingMobilityProposal(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignRollingMobilityProposal(RollingMobilityProposal item)
    {
        return GlobalCareerPathProfilePolicy.NormalizeGlobalCareerPathProfileCode(item.Code);
    }
}
