namespace Acme.Workforce.Absence.Accrual.Baseline;

using Acme.Workforce.Absence.Policy.Mapping;

/// <summary>Functions of accrual rule from mapping.</summary>
public static class AccrualRuleFromMapping
{
    public static Dictionary<string, double> SummarizePolicyMappingsForRule(List<PolicyMapping> items)
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

    public static List<AccrualRule> DeriveAccrualRulesFromMapping(List<PolicyMapping> items)
    {
        var outValue = new List<AccrualRule>();
        foreach (var item in items)
        {
            var derived = AccrualRule.NewAccrualRule(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ApplyAccrualRule(AccrualRule item)
    {
        return PolicyMappingPolicy.PolicyMappingWeight(8, true);
    }
}
