namespace Acme.Workforce.Orgdesign.Scenario.Audit;

using Acme.Workforce.Orgdesign.CostModel.AdaptiveModelPolicy;

/// <summary>Functions of default scenario slot from policy.</summary>
public static class DefaultScenarioSlotFromPolicy
{
    public static Dictionary<string, double> SummarizePendingCostModelPoliciesForSlot(List<PendingCostModelPolicy> items)
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

    public static List<DefaultScenarioSlot> DeriveDefaultScenarioSlotsFromPolicy(List<PendingCostModelPolicy> items)
    {
        var outValue = new List<DefaultScenarioSlot>();
        foreach (var item in items)
        {
            var derived = DefaultScenarioSlot.NewDefaultScenarioSlot(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double BalanceDefaultScenarioSlot(DefaultScenarioSlot item)
    {
        return CostModelDraftPolicy.CostModelDraftWeight(8, true);
    }
}
