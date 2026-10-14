namespace Acme.Workforce.Orgdesign.Reorg.Snapshot;

using Acme.Workforce.Orgdesign.CostModel.Summary;

/// <summary>Functions of seasonal reorg snapshot from checkpoint.</summary>
public static class SeasonalReorgSnapshotFromCheckpoint
{
    public static Dictionary<string, double> SummarizeStrictCostModelCheckpointsForSnapshot(List<StrictCostModelCheckpoint> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<SeasonalReorgSnapshot> DeriveSeasonalReorgSnapshotsFromCheckpoint(List<StrictCostModelCheckpoint> items)
    {
        var outValue = new List<SeasonalReorgSnapshot>();
        foreach (var item in items)
        {
            var derived = SeasonalReorgSnapshot.NewSeasonalReorgSnapshot(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double BalanceSeasonalReorgSnapshot(SeasonalReorgSnapshot item)
    {
        return StrictCostModelCheckpointPolicy.StrictCostModelCheckpointWeight(2, true);
    }
}
