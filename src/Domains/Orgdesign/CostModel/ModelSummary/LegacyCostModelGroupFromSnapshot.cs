namespace Acme.Workforce.Orgdesign.CostModel.ModelSummary;

using Acme.Workforce.Orgdesign.Scenario.Snapshot;

/// <summary>Functions of legacy cost model group from snapshot.</summary>
public static class LegacyCostModelGroupFromSnapshot
{
    public static Dictionary<string, double> SummarizeLegacyScenarioSnapshotsForGroup(List<LegacyScenarioSnapshot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<LegacyCostModelGroup> DeriveLegacyCostModelGroupsFromSnapshot(List<LegacyScenarioSnapshot> items)
    {
        var outValue = new List<LegacyCostModelGroup>();
        foreach (var item in items)
        {
            var derived = LegacyCostModelGroup.NewLegacyCostModelGroup(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectLegacyCostModelGroup(LegacyCostModelGroup item)
    {
        return RegionalScenarioBatchPolicy.RegionalScenarioBatchBucket(item.AdjustedExposure());
    }
}
