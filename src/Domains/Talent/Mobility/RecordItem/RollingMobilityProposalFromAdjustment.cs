namespace Acme.Workforce.Talent.Mobility.RecordItem;

using Acme.Workforce.Talent.Pool.Adjustment;

/// <summary>Functions of rolling mobility proposal from adjustment.</summary>
public static class RollingMobilityProposalFromAdjustment
{
    public static Dictionary<string, double> SummarizePoolAdjustmentsForProposal(List<PoolAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<RollingMobilityProposal> DeriveRollingMobilityProposalsFromAdjustment(List<PoolAdjustment> items)
    {
        var outValue = new List<RollingMobilityProposal>();
        foreach (var item in items)
        {
            var derived = RollingMobilityProposal.NewRollingMobilityProposal(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AllocateRollingMobilityProposal(RollingMobilityProposal item)
    {
        return PoolRollupPolicy.PoolRollupWeight(1, true);
    }
}
