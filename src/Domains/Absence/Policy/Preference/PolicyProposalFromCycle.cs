namespace Acme.Workforce.Absence.Policy.Preference;

using Acme.Workforce.Absence.Accrual.Cycle;

/// <summary>Functions of policy proposal from cycle.</summary>
public static class PolicyProposalFromCycle
{
    public static Dictionary<string, double> SummarizeAccrualCyclesForProposal(List<AccrualCycle> items)
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

    public static List<PolicyProposal> DerivePolicyProposalsFromCycle(List<AccrualCycle> items)
    {
        var outValue = new List<PolicyProposal>();
        foreach (var item in items)
        {
            var derived = PolicyProposal.NewPolicyProposal(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizePolicyProposal(PolicyProposal item)
    {
        return AccrualPreferencePolicy.AccrualPreferenceBucket(item.NetExposure());
    }
}
