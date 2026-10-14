namespace Acme.Workforce.Compensation.PayBand.Quota;

using Acme.Workforce.Compensation.PayBand.Milestone;

/// <summary>Functions of strict pay band preference from request.</summary>
public static class StrictPayBandPreferenceFromRequest
{
    public static Dictionary<string, double> SummarizeLegacyPayBandRequestsForPreference(List<LegacyPayBandRequest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<StrictPayBandPreference> DeriveStrictPayBandPreferencesFromRequest(List<LegacyPayBandRequest> items)
    {
        var outValue = new List<StrictPayBandPreference>();
        foreach (var item in items)
        {
            var derived = StrictPayBandPreference.NewStrictPayBandPreference(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AssignStrictPayBandPreference(StrictPayBandPreference item)
    {
        return PrimaryPayBandCheckpointPolicy.PrimaryPayBandCheckpointWeight(4, true);
    }
}
