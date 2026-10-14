namespace Acme.Workforce.Workplace.Occupancy.Estimate;

using Acme.Workforce.Workplace.Booking.OverrideItem;

/// <summary>Functions of manual occupancy agreement from override.</summary>
public static class ManualOccupancyAgreementFromOverride
{
    public static Dictionary<string, double> SummarizeBookingOverridesForAgreement(List<BookingOverride> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<ManualOccupancyAgreement> DeriveManualOccupancyAgreementsFromOverride(List<BookingOverride> items)
    {
        var outValue = new List<ManualOccupancyAgreement>();
        foreach (var item in items)
        {
            var derived = ManualOccupancyAgreement.NewManualOccupancyAgreement(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyManualOccupancyAgreement(ManualOccupancyAgreement item)
    {
        return GlobalBookingRulePolicy.GlobalBookingRuleBucket(item.ProjectedLoad());
    }
}
