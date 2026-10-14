namespace Acme.Workforce.Talent.Mobility.MobilityRecord;

using Acme.Workforce.Talent.Nomination.Segment;

/// <summary>Functions of weighted mobility record from forecast.</summary>
public static class WeightedMobilityRecordFromForecast
{
    public static Dictionary<string, double> SummarizeBatchedNominationForecastsForRecord(List<BatchedNominationForecast> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<WeightedMobilityRecord> DeriveWeightedMobilityRecordsFromForecast(List<BatchedNominationForecast> items)
    {
        var outValue = new List<WeightedMobilityRecord>();
        foreach (var item in items)
        {
            var derived = WeightedMobilityRecord.NewWeightedMobilityRecord(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double BalanceWeightedMobilityRecord(WeightedMobilityRecord item)
    {
        return BatchedNominationForecastPolicy.BatchedNominationForecastWeight(4, true);
    }
}
