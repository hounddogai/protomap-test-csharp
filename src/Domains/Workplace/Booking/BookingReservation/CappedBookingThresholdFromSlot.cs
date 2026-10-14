namespace Acme.Workforce.Workplace.Booking.BookingReservation;

using Acme.Workforce.Workplace.Desk.Draft;

/// <summary>Functions of capped booking threshold from slot.</summary>
public static class CappedBookingThresholdFromSlot
{
    public static Dictionary<string, double> SummarizeLegacyDeskSlotsForThreshold(List<LegacyDeskSlot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<CappedBookingThreshold> DeriveCappedBookingThresholdsFromSlot(List<LegacyDeskSlot> items)
    {
        var outValue = new List<CappedBookingThreshold>();
        foreach (var item in items)
        {
            var derived = CappedBookingThreshold.NewCappedBookingThreshold(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveCappedBookingThreshold(CappedBookingThreshold item)
    {
        return AdaptiveDeskClaimPolicy.AdaptiveDeskClaimBucket(item.AdjustedValue());
    }
}
