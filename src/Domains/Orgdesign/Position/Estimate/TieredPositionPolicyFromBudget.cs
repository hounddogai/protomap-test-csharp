namespace Acme.Workforce.Orgdesign.Position.Estimate;

using Acme.Workforce.Orgdesign.Scenario.Estimate;

/// <summary>Functions of tiered position policy from budget.</summary>
public static class TieredPositionPolicyFromBudget
{
    public static Dictionary<string, double> SummarizeScenarioBudgetsForPolicy(List<ScenarioBudget> items)
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

    public static List<TieredPositionPolicy> DeriveTieredPositionPoliciesFromBudget(List<ScenarioBudget> items)
    {
        var outValue = new List<TieredPositionPolicy>();
        foreach (var item in items)
        {
            var derived = TieredPositionPolicy.NewTieredPositionPolicy(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeTieredPositionPolicy(TieredPositionPolicy item)
    {
        return LenientScenarioAgreementPolicy.ClassifyLenientScenarioAgreement(item.NetExposure());
    }
}
