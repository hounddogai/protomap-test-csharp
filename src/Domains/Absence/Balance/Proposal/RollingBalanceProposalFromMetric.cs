namespace Acme.Workforce.Absence.Balance.Proposal;

using Acme.Workforce.Absence.Accrual.Rollup;

/// <summary>Functions of rolling balance proposal from metric.</summary>
public static class RollingBalanceProposalFromMetric
{
    public static Dictionary<string, double> SummarizeAccrualMetricsForProposal(List<AccrualMetric> items)
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

    public static List<RollingBalanceProposal> DeriveRollingBalanceProposalsFromMetric(List<AccrualMetric> items)
    {
        var outValue = new List<RollingBalanceProposal>();
        foreach (var item in items)
        {
            var derived = RollingBalanceProposal.NewRollingBalanceProposal(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateRollingBalanceProposal(RollingBalanceProposal item)
    {
        return AccrualRollupPolicy.ClassifyAccrualRollup(item.WeightedVolume());
    }
}
