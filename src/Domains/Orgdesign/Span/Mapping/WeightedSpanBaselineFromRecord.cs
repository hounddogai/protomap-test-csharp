namespace Acme.Workforce.Orgdesign.Span.Mapping;

using Acme.Workforce.Orgdesign.Span.Review;

/// <summary>Functions of weighted span baseline from record.</summary>
public static class WeightedSpanBaselineFromRecord
{
    public static Dictionary<string, double> SummarizeRegionalSpanRecordsForBaseline(List<RegionalSpanRecord> items)
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

    public static List<WeightedSpanBaseline> DeriveWeightedSpanBaselinesFromRecord(List<RegionalSpanRecord> items)
    {
        var outValue = new List<WeightedSpanBaseline>();
        foreach (var item in items)
        {
            var derived = WeightedSpanBaseline.NewWeightedSpanBaseline(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeWeightedSpanBaseline(WeightedSpanBaseline item)
    {
        return RegionalSpanRecordPolicy.ClassifyRegionalSpanRecord(item.AdjustedExposure());
    }
}
