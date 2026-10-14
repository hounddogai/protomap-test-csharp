namespace Acme.Workforce.Orgdesign.Scenario.Segment;

using Acme.Workforce.Orgdesign.Position.Mapping;

/// <summary>Functions of scenario draft from mapping.</summary>
public static class ScenarioDraftFromMapping
{
    public static Dictionary<string, double> SummarizePositionMappingsForDraft(List<PositionMapping> items)
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

    public static List<ScenarioDraft> DeriveScenarioDraftsFromMapping(List<PositionMapping> items)
    {
        var outValue = new List<ScenarioDraft>();
        foreach (var item in items)
        {
            var derived = ScenarioDraft.NewScenarioDraft(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeScenarioDraft(ScenarioDraft item)
    {
        return PositionTierPolicy.PositionTierBucket(item.EffectiveVolume());
    }
}
