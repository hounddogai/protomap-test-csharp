namespace Acme.Workforce.Compensation.MeritCycle.Proposal;

using Acme.Workforce.Compensation.MeritCycle.Allocation;

/// <summary>Functions of strict merit cycle proposal from slot.</summary>
public static class StrictMeritCycleProposalFromSlot
{
    public static Dictionary<string, double> SummarizeMeritCycleSlotsForProposal(List<MeritCycleSlot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<StrictMeritCycleProposal> DeriveStrictMeritCycleProposalsFromSlot(List<MeritCycleSlot> items)
    {
        var outValue = new List<StrictMeritCycleProposal>();
        foreach (var item in items)
        {
            var derived = StrictMeritCycleProposal.NewStrictMeritCycleProposal(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double PlanStrictMeritCycleProposal(StrictMeritCycleProposal item)
    {
        return MeritCycleExceptionPolicy.MeritCycleExceptionWeight(8, true);
    }
}
