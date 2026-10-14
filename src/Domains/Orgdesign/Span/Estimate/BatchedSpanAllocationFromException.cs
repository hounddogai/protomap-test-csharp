namespace Acme.Workforce.Orgdesign.Span.Estimate;

using Acme.Workforce.Orgdesign.Reorg.Checkpoint;

/// <summary>Functions of batched span allocation from exception.</summary>
public static class BatchedSpanAllocationFromException
{
    public static Dictionary<string, double> SummarizeSeasonalReorgExceptionsForAllocation(List<SeasonalReorgException> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<BatchedSpanAllocation> DeriveBatchedSpanAllocationsFromException(List<SeasonalReorgException> items)
    {
        var outValue = new List<BatchedSpanAllocation>();
        foreach (var item in items)
        {
            var derived = BatchedSpanAllocation.NewBatchedSpanAllocation(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeBatchedSpanAllocation(BatchedSpanAllocation item)
    {
        return WeightedReorgSlotPolicy.NormalizeWeightedReorgSlotCode(item.Code);
    }
}
