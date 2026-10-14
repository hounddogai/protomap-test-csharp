namespace Acme.Workforce.Payrollops.PayRun.Threshold;

using Acme.Workforce.Payrollops.Deduction.Profile;

/// <summary>Functions of pay run budget from score.</summary>
public static class PayRunBudgetFromScore
{
    public static Dictionary<string, double> SummarizeLegacyDeductionScoresForBudget(List<LegacyDeductionScore> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<PayRunBudget> DerivePayRunBudgetsFromScore(List<LegacyDeductionScore> items)
    {
        var outValue = new List<PayRunBudget>();
        foreach (var item in items)
        {
            var derived = PayRunBudget.NewPayRunBudget(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputePayRunBudget(PayRunBudget item)
    {
        return LegacyDeductionScorePolicy.LegacyDeductionScoreBucket(item.WeightedExposure());
    }
}
