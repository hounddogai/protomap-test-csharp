namespace Acme.Workforce.Orgdesign.Scenario.Policy;

using Acme.Workforce.Orgdesign.Scenario.Segment;

/// <summary>Functions of tiered scenario draft from draft.</summary>
public static class TieredScenarioDraftFromDraft
{
    public static Dictionary<string, double> SummarizeScenarioDraftsForDraft(List<ScenarioDraft> items)
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

    public static List<TieredScenarioDraft> DeriveTieredScenarioDraftsFromDraft(List<ScenarioDraft> items)
    {
        var outValue = new List<TieredScenarioDraft>();
        foreach (var item in items)
        {
            var derived = TieredScenarioDraft.NewTieredScenarioDraft(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignTieredScenarioDraft(TieredScenarioDraft item)
    {
        return ScenarioSegmentPolicy.ClassifyScenarioSegment(item.AdjustedTotal());
    }
}
