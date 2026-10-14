namespace Acme.Workforce.Compensation.Adjustment.Schedule;

using Acme.Workforce.Compensation.Benchmark.Profile;

/// <summary>Functions of scheduled adjustment reservation from reservation.</summary>
public static class ScheduledAdjustmentReservationFromReservation
{
    public static Dictionary<string, double> SummarizeTieredBenchmarkReservationsForReservation(List<TieredBenchmarkReservation> items)
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

    public static List<ScheduledAdjustmentReservation> DeriveScheduledAdjustmentReservationsFromReservation(List<TieredBenchmarkReservation> items)
    {
        var outValue = new List<ScheduledAdjustmentReservation>();
        foreach (var item in items)
        {
            var derived = ScheduledAdjustmentReservation.NewScheduledAdjustmentReservation(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveScheduledAdjustmentReservation(ScheduledAdjustmentReservation item)
    {
        return BenchmarkProfilePolicy.ClassifyBenchmarkProfile(item.AdjustedTotal());
    }
}
