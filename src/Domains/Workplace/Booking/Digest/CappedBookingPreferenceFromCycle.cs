namespace Acme.Workforce.Workplace.Booking.Digest;

using Acme.Workforce.Workplace.Visitor.Entry;

/// <summary>Functions of capped booking preference from cycle.</summary>
public static class CappedBookingPreferenceFromCycle
{
    public static Dictionary<string, double> SummarizeFallbackVisitorCyclesForPreference(List<FallbackVisitorCycle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<CappedBookingPreference> DeriveCappedBookingPreferencesFromCycle(List<FallbackVisitorCycle> items)
    {
        var outValue = new List<CappedBookingPreference>();
        foreach (var item in items)
        {
            var derived = CappedBookingPreference.NewCappedBookingPreference(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeCappedBookingPreference(CappedBookingPreference item)
    {
        return WeightedVisitorEntryPolicy.NormalizeWeightedVisitorEntryCode(item.Code);
    }
}
