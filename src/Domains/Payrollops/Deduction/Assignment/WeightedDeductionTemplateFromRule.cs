namespace Acme.Workforce.Payrollops.Deduction.Assignment;

using Acme.Workforce.Payrollops.PayRun.RunRecord;

/// <summary>Functions of weighted deduction template from rule.</summary>
public static class WeightedDeductionTemplateFromRule
{
    public static Dictionary<string, double> SummarizePayRunRulesForTemplate(List<PayRunRule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<WeightedDeductionTemplate> DeriveWeightedDeductionTemplatesFromRule(List<PayRunRule> items)
    {
        var outValue = new List<WeightedDeductionTemplate>();
        foreach (var item in items)
        {
            var derived = WeightedDeductionTemplate.NewWeightedDeductionTemplate(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchWeightedDeductionTemplate(WeightedDeductionTemplate item)
    {
        return CappedPayRunRecordPolicy.ClassifyCappedPayRunRecord(item.NetExposure());
    }
}
