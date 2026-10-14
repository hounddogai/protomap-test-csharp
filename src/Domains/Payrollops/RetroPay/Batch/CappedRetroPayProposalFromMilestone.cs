namespace Acme.Workforce.Payrollops.RetroPay.Batch;

using Acme.Workforce.Payrollops.PayRun.RunVariance;

/// <summary>Functions of capped retro pay proposal from milestone.</summary>
public static class CappedRetroPayProposalFromMilestone
{
    public static Dictionary<string, double> SummarizeTieredPayRunMilestonesForProposal(List<TieredPayRunMilestone> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<CappedRetroPayProposal> DeriveCappedRetroPayProposalsFromMilestone(List<TieredPayRunMilestone> items)
    {
        var outValue = new List<CappedRetroPayProposal>();
        foreach (var item in items)
        {
            var derived = CappedRetroPayProposal.NewCappedRetroPayProposal(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanCappedRetroPayProposal(CappedRetroPayProposal item)
    {
        return TieredPayRunMilestonePolicy.ClassifyTieredPayRunMilestone(item.ProjectedValue());
    }
}
