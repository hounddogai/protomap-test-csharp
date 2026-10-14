namespace Acme.Workforce.Orgdesign.CostModel.Summary;

using Acme.Workforce.Orgdesign.CostModel.ModelManifest;

/// <summary>Functions of strict cost model checkpoint from batch.</summary>
public static class StrictCostModelCheckpointFromBatch
{
    public static Dictionary<string, double> SummarizeScheduledCostModelBatchesForCheckpoint(List<ScheduledCostModelBatch> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<StrictCostModelCheckpoint> DeriveStrictCostModelCheckpointsFromBatch(List<ScheduledCostModelBatch> items)
    {
        var outValue = new List<StrictCostModelCheckpoint>();
        foreach (var item in items)
        {
            var derived = StrictCostModelCheckpoint.NewStrictCostModelCheckpoint(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreStrictCostModelCheckpoint(StrictCostModelCheckpoint item)
    {
        return ScheduledCostModelBatchPolicy.NormalizeScheduledCostModelBatchCode(item.Code);
    }
}
