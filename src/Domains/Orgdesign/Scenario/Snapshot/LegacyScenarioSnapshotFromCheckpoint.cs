namespace Acme.Workforce.Orgdesign.Scenario.Snapshot;

using Acme.Workforce.Orgdesign.Scenario.Notice;

/// <summary>Functions of legacy scenario snapshot from checkpoint.</summary>
public static class LegacyScenarioSnapshotFromCheckpoint
{
    public static Dictionary<string, double> SummarizePendingScenarioCheckpointsForSnapshot(List<PendingScenarioCheckpoint> items)
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

    public static List<LegacyScenarioSnapshot> DeriveLegacyScenarioSnapshotsFromCheckpoint(List<PendingScenarioCheckpoint> items)
    {
        var outValue = new List<LegacyScenarioSnapshot>();
        foreach (var item in items)
        {
            var derived = LegacyScenarioSnapshot.NewLegacyScenarioSnapshot(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SummarizeLegacyScenarioSnapshot(LegacyScenarioSnapshot item)
    {
        return ScenarioNoticePolicy.ScenarioNoticeWeight(9, true);
    }
}
