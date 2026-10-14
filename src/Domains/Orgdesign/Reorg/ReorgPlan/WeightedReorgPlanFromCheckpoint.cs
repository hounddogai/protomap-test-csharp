namespace Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

using Acme.Workforce.Orgdesign.Reorg.Checkpoint;

/// <summary>Functions of weighted reorg plan from checkpoint.</summary>
public static class WeightedReorgPlanFromCheckpoint
{
    public static Dictionary<string, double> SummarizeBatchedReorgCheckpointsForPlan(List<BatchedReorgCheckpoint> items)
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

    public static List<WeightedReorgPlan> DeriveWeightedReorgPlansFromCheckpoint(List<BatchedReorgCheckpoint> items)
    {
        var outValue = new List<WeightedReorgPlan>();
        foreach (var item in items)
        {
            var derived = WeightedReorgPlan.NewWeightedReorgPlan(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyWeightedReorgPlan(WeightedReorgPlan item)
    {
        return WeightedReorgSlotPolicy.NormalizeWeightedReorgSlotCode(item.Code);
    }
}
