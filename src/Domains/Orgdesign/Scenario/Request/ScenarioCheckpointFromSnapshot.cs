namespace Acme.Workforce.Orgdesign.Scenario.Request;

using Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

/// <summary>Functions of scenario checkpoint from snapshot.</summary>
public static class ScenarioCheckpointFromSnapshot
{
    public static Dictionary<string, double> SummarizeAdaptiveReorgSnapshotsForCheckpoint(List<AdaptiveReorgSnapshot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<ScenarioCheckpoint> DeriveScenarioCheckpointsFromSnapshot(List<AdaptiveReorgSnapshot> items)
    {
        var outValue = new List<ScenarioCheckpoint>();
        foreach (var item in items)
        {
            var derived = ScenarioCheckpoint.NewScenarioCheckpoint(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double RankScenarioCheckpoint(ScenarioCheckpoint item)
    {
        return ReorgAdjustmentPolicy.ReorgAdjustmentWeight(3, true);
    }
}
