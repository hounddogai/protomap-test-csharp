namespace Acme.Workforce.Orgdesign.Scenario.Policy;

using Acme.Workforce.Orgdesign.Headcount.OverrideItem;

/// <summary>Functions of global scenario policy from summary.</summary>
public static class GlobalScenarioPolicyFromSummary
{
    public static Dictionary<string, double> SummarizeHeadcountSummariesForPolicy(List<HeadcountSummary> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<GlobalScenarioPolicy> DeriveGlobalScenarioPoliciesFromSummary(List<HeadcountSummary> items)
    {
        var outValue = new List<GlobalScenarioPolicy>();
        foreach (var item in items)
        {
            var derived = GlobalScenarioPolicy.NewGlobalScenarioPolicy(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchGlobalScenarioPolicy(GlobalScenarioPolicy item)
    {
        return HeadcountOverridePolicy.NormalizeHeadcountOverrideCode(item.Code);
    }
}
