namespace Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

using Acme.Workforce.Orgdesign.Scenario.Policy;

/// <summary>Functions of scenario estimate from policy.</summary>
public static class ScenarioEstimateFromPolicy
{
    public static Dictionary<string, double> SummarizeGlobalScenarioPoliciesForEstimate(List<GlobalScenarioPolicy> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<ScenarioEstimate> DeriveScenarioEstimatesFromPolicy(List<GlobalScenarioPolicy> items)
    {
        var outValue = new List<ScenarioEstimate>();
        foreach (var item in items)
        {
            var derived = ScenarioEstimate.NewScenarioEstimate(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double BalanceScenarioEstimate(ScenarioEstimate item)
    {
        return TieredScenarioDraftPolicy.TieredScenarioDraftWeight(8, true);
    }
}
