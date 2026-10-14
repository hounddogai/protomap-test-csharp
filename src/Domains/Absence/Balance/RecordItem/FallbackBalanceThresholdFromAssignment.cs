namespace Acme.Workforce.Absence.Balance.RecordItem;

using Acme.Workforce.Absence.Policy.PolicyRule;

/// <summary>Functions of fallback balance threshold from assignment.</summary>
public static class FallbackBalanceThresholdFromAssignment
{
    public static Dictionary<string, double> SummarizeStrictPolicyAssignmentsForThreshold(List<StrictPolicyAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<FallbackBalanceThreshold> DeriveFallbackBalanceThresholdsFromAssignment(List<StrictPolicyAssignment> items)
    {
        var outValue = new List<FallbackBalanceThreshold>();
        foreach (var item in items)
        {
            var derived = FallbackBalanceThreshold.NewFallbackBalanceThreshold(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchFallbackBalanceThreshold(FallbackBalanceThreshold item)
    {
        return TieredPolicyRulePolicy.NormalizeTieredPolicyRuleCode(item.Code);
    }
}
