namespace Acme.Workforce.Orgdesign.Scenario.Segment;

using Acme.Workforce.Orgdesign.CostModel.Score;

/// <summary>Functions of scenario draft from slot.</summary>
public static class ScenarioDraftFromSlot
{
    public static Dictionary<string, double> SummarizeCostModelSlotsForDraft(List<CostModelSlot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<ScenarioDraft> DeriveScenarioDraftsFromSlot(List<CostModelSlot> items)
    {
        var outValue = new List<ScenarioDraft>();
        foreach (var item in items)
        {
            var derived = ScenarioDraft.NewScenarioDraft(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeScenarioDraft(ScenarioDraft item)
    {
        return CostModelSlotPolicy.ClassifyCostModelSlot(item.EffectiveVolume());
    }
}
