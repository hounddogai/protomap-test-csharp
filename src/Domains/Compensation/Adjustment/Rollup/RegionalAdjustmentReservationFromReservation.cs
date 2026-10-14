namespace Acme.Workforce.Compensation.Adjustment.Rollup;

using Acme.Workforce.Compensation.Adjustment.Schedule;

/// <summary>Functions of regional adjustment reservation from reservation.</summary>
public static class RegionalAdjustmentReservationFromReservation
{
    public static Dictionary<string, double> SummarizeScheduledAdjustmentReservationsForReservation(List<ScheduledAdjustmentReservation> items)
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

    public static List<RegionalAdjustmentReservation> DeriveRegionalAdjustmentReservationsFromReservation(List<ScheduledAdjustmentReservation> items)
    {
        var outValue = new List<RegionalAdjustmentReservation>();
        foreach (var item in items)
        {
            var derived = RegionalAdjustmentReservation.NewRegionalAdjustmentReservation(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeRegionalAdjustmentReservation(RegionalAdjustmentReservation item)
    {
        return ScheduledAdjustmentReservationPolicy.NormalizeScheduledAdjustmentReservationCode(item.Code);
    }
}
