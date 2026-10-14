namespace Acme.Workforce.Workplace.Occupancy.Mapping;

using Acme.Workforce.Workplace.Visitor.Checkpoint;

/// <summary>Functions of batched occupancy mapping from checkpoint.</summary>
public static class BatchedOccupancyMappingFromCheckpoint
{
    public static Dictionary<string, double> SummarizeLegacyVisitorCheckpointsForMapping(List<LegacyVisitorCheckpoint> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<BatchedOccupancyMapping> DeriveBatchedOccupancyMappingsFromCheckpoint(List<LegacyVisitorCheckpoint> items)
    {
        var outValue = new List<BatchedOccupancyMapping>();
        foreach (var item in items)
        {
            var derived = BatchedOccupancyMapping.NewBatchedOccupancyMapping(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveBatchedOccupancyMapping(BatchedOccupancyMapping item)
    {
        return LegacyVisitorCheckpointPolicy.ClassifyLegacyVisitorCheckpoint(item.ProjectedExposure());
    }
}
