namespace Acme.Workforce.Compensation.EquityGrant.Metric;

using Acme.Workforce.Compensation.MeritCycle.Proposal;

/// <summary>Functions of equity grant plan from proposal.</summary>
public static class EquityGrantPlanFromProposal
{
    public static Dictionary<string, double> SummarizeStrictMeritCycleProposalsForPlan(List<StrictMeritCycleProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<EquityGrantPlan> DeriveEquityGrantPlansFromProposal(List<StrictMeritCycleProposal> items)
    {
        var outValue = new List<EquityGrantPlan>();
        foreach (var item in items)
        {
            var derived = EquityGrantPlan.NewEquityGrantPlan(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveEquityGrantPlan(EquityGrantPlan item)
    {
        return MeritCycleGroupPolicy.NormalizeMeritCycleGroupCode(item.Code);
    }
}
