namespace Acme.Workforce.Payrollops.RetroPay.Forecast;

using Acme.Workforce.Payrollops.Deduction.Baseline;

/// <summary>Functions of retro pay rule from baseline.</summary>
public static class RetroPayRuleFromBaseline
{
    public static Dictionary<string, double> SummarizeDeductionBaselinesForRule(List<DeductionBaseline> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<RetroPayRule> DeriveRetroPayRulesFromBaseline(List<DeductionBaseline> items)
    {
        var outValue = new List<RetroPayRule>();
        foreach (var item in items)
        {
            var derived = RetroPayRule.NewRetroPayRule(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectRetroPayRule(RetroPayRule item)
    {
        return DeductionBaselinePolicy.ClassifyDeductionBaseline(item.MeasuredValue());
    }
}
