namespace Acme.Workforce.Absence.Accrual.Batch;

using Acme.Workforce.Absence.Accrual.Cycle;
using Acme.Workforce.Absence.Policy.Preference;

/// <summary>Functions of accrual batch from preference.</summary>
public static class AccrualBatchFromPreference
{
    public static Dictionary<string, double> SummarizeAccrualPreferencesForBatch(List<AccrualPreference> items)
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

    public static List<AccrualBatch> DeriveAccrualBatchesFromPreference(List<AccrualPreference> items)
    {
        var outValue = new List<AccrualBatch>();
        foreach (var item in items)
        {
            var derived = AccrualBatch.NewAccrualBatch(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SplitAccrualBatch(AccrualBatch item)
    {
        return AccrualScorePolicy.AccrualScoreWeight(4, true);
    }

    public static Dictionary<string, double> SummarizePolicyPreferencesForBatch(List<PolicyPreference> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<AccrualBatch> DeriveAccrualBatchesFromPreferenceExtra(List<PolicyPreference> items)
    {
        var outValue = new List<AccrualBatch>();
        foreach (var item in items)
        {
            var derived = AccrualBatch.NewAccrualBatch(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileAccrualBatch(AccrualBatch item)
    {
        return PolicyProposalPolicy.ClassifyPolicyProposal(item.NetExposure());
    }
}
