namespace Acme.Workforce.Workplace.AccessBadge.Budget;

using Acme.Workforce.Workplace.AccessBadge.Threshold;

/// <summary>Functions of access badge adjustment from reservation.</summary>
public static class AccessBadgeAdjustmentFromReservation
{
    public static Dictionary<string, double> SummarizeAccessBadgeReservationsForAdjustment(List<AccessBadgeReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<AccessBadgeAdjustment> DeriveAccessBadgeAdjustmentsFromReservation(List<AccessBadgeReservation> items)
    {
        var outValue = new List<AccessBadgeAdjustment>();
        foreach (var item in items)
        {
            var derived = AccessBadgeAdjustment.NewAccessBadgeAdjustment(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeAccessBadgeAdjustment(AccessBadgeAdjustment item)
    {
        return WeightedAccessBadgeExceptionPolicy.WeightedAccessBadgeExceptionBucket(item.WeightedTotal());
    }
}
