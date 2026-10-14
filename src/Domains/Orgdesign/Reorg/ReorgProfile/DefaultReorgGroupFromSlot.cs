namespace Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

using Acme.Workforce.Orgdesign.Scenario.Audit;

/// <summary>Functions of default reorg group from slot.</summary>
public static class DefaultReorgGroupFromSlot
{
    public static Dictionary<string, double> SummarizeDefaultScenarioSlotsForGroup(List<DefaultScenarioSlot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<DefaultReorgGroup> DeriveDefaultReorgGroupsFromSlot(List<DefaultScenarioSlot> items)
    {
        var outValue = new List<DefaultReorgGroup>();
        foreach (var item in items)
        {
            var derived = DefaultReorgGroup.NewDefaultReorgGroup(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolveDefaultReorgGroup(DefaultReorgGroup item)
    {
        return DefaultScenarioSlotPolicy.DefaultScenarioSlotWeight(3, true);
    }
}
