namespace Acme.Workforce.Compensation.BonusPool.Rule;

using Acme.Workforce.Compensation.Benchmark.AdaptiveBenchmarkAdjustment;

/// <summary>Functions of bonus pool rule from notice.</summary>
public static class BonusPoolRuleFromNotice
{
    public static Dictionary<string, double> SummarizeAdaptiveBenchmarkNoticesForRule(List<AdaptiveBenchmarkNotice> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<BonusPoolRule> DeriveBonusPoolRulesFromNotice(List<AdaptiveBenchmarkNotice> items)
    {
        var outValue = new List<BonusPoolRule>();
        foreach (var item in items)
        {
            var derived = BonusPoolRule.NewBonusPoolRule(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileBonusPoolRule(BonusPoolRule item)
    {
        return SeasonalBenchmarkGroupPolicy.ClassifySeasonalBenchmarkGroup(item.MeasuredValue());
    }
}
