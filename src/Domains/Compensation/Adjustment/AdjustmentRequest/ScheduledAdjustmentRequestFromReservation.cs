namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

using Acme.Workforce.Compensation.Adjustment.BatchedAdjustmentRule;

/// <summary>Functions of scheduled adjustment request from reservation.</summary>
public static class ScheduledAdjustmentRequestFromReservation
{
    public static Dictionary<string, double> SummarizeSeasonalAdjustmentReservationsForRequest(List<SeasonalAdjustmentReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<ScheduledAdjustmentRequest> DeriveScheduledAdjustmentRequestsFromReservation(List<SeasonalAdjustmentReservation> items)
    {
        var outValue = new List<ScheduledAdjustmentRequest>();
        foreach (var item in items)
        {
            var derived = ScheduledAdjustmentRequest.NewScheduledAdjustmentRequest(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double RankScheduledAdjustmentRequest(ScheduledAdjustmentRequest item)
    {
        return SeasonalAdjustmentReservationPolicy.SeasonalAdjustmentReservationWeight(9, true);
    }
}
