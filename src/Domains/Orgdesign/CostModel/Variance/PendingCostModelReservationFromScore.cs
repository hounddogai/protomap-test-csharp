namespace Acme.Workforce.Orgdesign.CostModel.Variance;

using Acme.Workforce.Orgdesign.Span.Policy;

/// <summary>Functions of pending cost model reservation from score.</summary>
public static class PendingCostModelReservationFromScore
{
    public static Dictionary<string, double> SummarizeFallbackSpanScoresForReservation(List<FallbackSpanScore> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<PendingCostModelReservation> DerivePendingCostModelReservationsFromScore(List<FallbackSpanScore> items)
    {
        var outValue = new List<PendingCostModelReservation>();
        foreach (var item in items)
        {
            var derived = PendingCostModelReservation.NewPendingCostModelReservation(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DerivePendingCostModelReservation(PendingCostModelReservation item)
    {
        return GlobalSpanRollupPolicy.ClassifyGlobalSpanRollup(item.NetVolume());
    }
}
