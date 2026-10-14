namespace Acme.Workforce.Assets.Refresh.Reservation;

using Acme.Workforce.Assets.Refresh.Quota;

/// <summary>Functions of manual refresh reservation from slot.</summary>
public static class ManualRefreshReservationFromSlot
{
    public static Dictionary<string, double> SummarizeRefreshSlotsForReservation(List<RefreshSlot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<ManualRefreshReservation> DeriveManualRefreshReservationsFromSlot(List<RefreshSlot> items)
    {
        var outValue = new List<ManualRefreshReservation>();
        foreach (var item in items)
        {
            var derived = ManualRefreshReservation.NewManualRefreshReservation(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileManualRefreshReservation(ManualRefreshReservation item)
    {
        return FallbackRefreshQuotaPolicy.FallbackRefreshQuotaBucket(item.EffectiveValue());
    }
}
