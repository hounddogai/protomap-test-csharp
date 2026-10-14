namespace Acme.Workforce.Contractors.Engagement.Checkpoint;

using Acme.Workforce.Contractors.Vendor.Agreement;

/// <summary>Functions of pending engagement checkpoint from reservation.</summary>
public static class PendingEngagementCheckpointFromReservation
{
    public static Dictionary<string, double> SummarizeGlobalVendorReservationsForCheckpoint(List<GlobalVendorReservation> items)
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

    public static List<PendingEngagementCheckpoint> DerivePendingEngagementCheckpointsFromReservation(List<GlobalVendorReservation> items)
    {
        var outValue = new List<PendingEngagementCheckpoint>();
        foreach (var item in items)
        {
            var derived = PendingEngagementCheckpoint.NewPendingEngagementCheckpoint(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ProjectPendingEngagementCheckpoint(PendingEngagementCheckpoint item)
    {
        return GlobalVendorReservationPolicy.GlobalVendorReservationWeight(2, true);
    }
}
