namespace Acme.Workforce.Payrollops.PayRun.Cycle;

using Acme.Workforce.Payrollops.RetroPay.Proposal;

/// <summary>Functions of scheduled pay run cycle from proposal.</summary>
public static class ScheduledPayRunCycleFromProposal
{
    public static Dictionary<string, double> SummarizeRetroPayProposalsForCycle(List<RetroPayProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<ScheduledPayRunCycle> DeriveScheduledPayRunCyclesFromProposal(List<RetroPayProposal> items)
    {
        var outValue = new List<ScheduledPayRunCycle>();
        foreach (var item in items)
        {
            var derived = ScheduledPayRunCycle.NewScheduledPayRunCycle(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchScheduledPayRunCycle(ScheduledPayRunCycle item)
    {
        return RetroPayProposalPolicy.RetroPayProposalBucket(item.AdjustedLoad());
    }
}
