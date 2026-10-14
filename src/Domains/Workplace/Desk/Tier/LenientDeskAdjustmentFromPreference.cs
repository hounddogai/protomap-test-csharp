namespace Acme.Workforce.Workplace.Desk.Tier;

using Acme.Workforce.Workplace.Booking.BookingReview;

/// <summary>Functions of lenient desk adjustment from preference.</summary>
public static class LenientDeskAdjustmentFromPreference
{
    public static Dictionary<string, double> SummarizeManualBookingPreferencesForAdjustment(List<ManualBookingPreference> items)
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

    public static List<LenientDeskAdjustment> DeriveLenientDeskAdjustmentsFromPreference(List<ManualBookingPreference> items)
    {
        var outValue = new List<LenientDeskAdjustment>();
        foreach (var item in items)
        {
            var derived = LenientDeskAdjustment.NewLenientDeskAdjustment(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignLenientDeskAdjustment(LenientDeskAdjustment item)
    {
        return PrimaryBookingTierPolicy.PrimaryBookingTierBucket(item.MeasuredValue());
    }
}
