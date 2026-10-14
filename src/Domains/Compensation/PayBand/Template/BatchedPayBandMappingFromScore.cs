namespace Acme.Workforce.Compensation.PayBand.Template;

using Acme.Workforce.Compensation.MeritCycle.Segment;

/// <summary>Functions of batched pay band mapping from score.</summary>
public static class BatchedPayBandMappingFromScore
{
    public static Dictionary<string, double> SummarizeMeritCycleScoresForMapping(List<MeritCycleScore> items)
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

    public static List<BatchedPayBandMapping> DeriveBatchedPayBandMappingsFromScore(List<MeritCycleScore> items)
    {
        var outValue = new List<BatchedPayBandMapping>();
        foreach (var item in items)
        {
            var derived = BatchedPayBandMapping.NewBatchedPayBandMapping(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SplitBatchedPayBandMapping(BatchedPayBandMapping item)
    {
        return MeritCycleScorePolicy.MeritCycleScoreWeight(9, true);
    }
}
