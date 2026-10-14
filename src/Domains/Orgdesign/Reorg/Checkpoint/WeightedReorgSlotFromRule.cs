namespace Acme.Workforce.Orgdesign.Reorg.Checkpoint;

using Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

/// <summary>Functions of weighted reorg slot from rule.</summary>
public static class WeightedReorgSlotFromRule
{
    public static Dictionary<string, double> SummarizeManualScenarioRulesForSlot(List<ManualScenarioRule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<WeightedReorgSlot> DeriveWeightedReorgSlotsFromRule(List<ManualScenarioRule> items)
    {
        var outValue = new List<WeightedReorgSlot>();
        foreach (var item in items)
        {
            var derived = WeightedReorgSlot.NewWeightedReorgSlot(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyWeightedReorgSlot(WeightedReorgSlot item)
    {
        return ManualScenarioRulePolicy.NormalizeManualScenarioRuleCode(item.Code);
    }
}
