namespace Acme.Workforce.Orgdesign.CostModel.Group;

using Acme.Workforce.Orgdesign.Span.Review;

/// <summary>Functions of fallback cost model score from record.</summary>
public static class FallbackCostModelScoreFromRecord
{
    public static Dictionary<string, double> SummarizeRegionalSpanRecordsForScore(List<RegionalSpanRecord> items)
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

    public static List<FallbackCostModelScore> DeriveFallbackCostModelScoresFromRecord(List<RegionalSpanRecord> items)
    {
        var outValue = new List<FallbackCostModelScore>();
        foreach (var item in items)
        {
            var derived = FallbackCostModelScore.NewFallbackCostModelScore(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ApplyFallbackCostModelScore(FallbackCostModelScore item)
    {
        return RegionalSpanRecordPolicy.RegionalSpanRecordWeight(8, true);
    }
}
