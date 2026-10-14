namespace Acme.Workforce.Workplace.Booking.Entry;

using Acme.Workforce.Workplace.Desk.Preference;

/// <summary>Functions of regional booking preference from preference.</summary>
public static class RegionalBookingPreferenceFromPreference
{
    public static Dictionary<string, double> SummarizeWeightedDeskPreferencesForPreference(List<WeightedDeskPreference> items)
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

    public static List<RegionalBookingPreference> DeriveRegionalBookingPreferencesFromPreference(List<WeightedDeskPreference> items)
    {
        var outValue = new List<RegionalBookingPreference>();
        foreach (var item in items)
        {
            var derived = RegionalBookingPreference.NewRegionalBookingPreference(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreRegionalBookingPreference(RegionalBookingPreference item)
    {
        return DeskDraftPolicy.NormalizeDeskDraftCode(item.Code);
    }
}
