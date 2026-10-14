namespace Acme.Workforce.Workplace.Visitor.OverrideItem;

using Acme.Workforce.Workplace.AccessBadge.Request;

/// <summary>Functions of pending visitor adjustment from reservation.</summary>
public static class PendingVisitorAdjustmentFromReservation
{
    public static Dictionary<string, double> SummarizeWeightedAccessBadgeReservationsForAdjustment(List<WeightedAccessBadgeReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<PendingVisitorAdjustment> DerivePendingVisitorAdjustmentsFromReservation(List<WeightedAccessBadgeReservation> items)
    {
        var outValue = new List<PendingVisitorAdjustment>();
        foreach (var item in items)
        {
            var derived = PendingVisitorAdjustment.NewPendingVisitorAdjustment(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyPendingVisitorAdjustment(PendingVisitorAdjustment item)
    {
        return WeightedAccessBadgeReservationPolicy.WeightedAccessBadgeReservationBucket(item.NetExposure());
    }
}
