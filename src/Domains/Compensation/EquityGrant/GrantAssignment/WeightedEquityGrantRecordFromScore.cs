namespace Acme.Workforce.Compensation.EquityGrant.GrantAssignment;

using Acme.Workforce.Compensation.PayBand.Score;

/// <summary>Functions of weighted equity grant record from score.</summary>
public static class WeightedEquityGrantRecordFromScore
{
    public static Dictionary<string, double> SummarizeFallbackPayBandScoresForRecord(List<FallbackPayBandScore> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<WeightedEquityGrantRecord> DeriveWeightedEquityGrantRecordsFromScore(List<FallbackPayBandScore> items)
    {
        var outValue = new List<WeightedEquityGrantRecord>();
        foreach (var item in items)
        {
            var derived = WeightedEquityGrantRecord.NewWeightedEquityGrantRecord(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ReconcileWeightedEquityGrantRecord(WeightedEquityGrantRecord item)
    {
        return FallbackPayBandScorePolicy.FallbackPayBandScoreWeight(5, true);
    }
}
