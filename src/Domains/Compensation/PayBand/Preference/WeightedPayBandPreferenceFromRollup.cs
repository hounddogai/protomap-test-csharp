namespace Acme.Workforce.Compensation.PayBand.Preference;

using Acme.Workforce.Compensation.PayBand.Tier;

/// <summary>Functions of weighted pay band preference from rollup.</summary>
public static class WeightedPayBandPreferenceFromRollup
{
    public static Dictionary<string, double> SummarizeLegacyPayBandRollupsForPreference(List<LegacyPayBandRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<WeightedPayBandPreference> DeriveWeightedPayBandPreferencesFromRollup(List<LegacyPayBandRollup> items)
    {
        var outValue = new List<WeightedPayBandPreference>();
        foreach (var item in items)
        {
            var derived = WeightedPayBandPreference.NewWeightedPayBandPreference(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeWeightedPayBandPreference(WeightedPayBandPreference item)
    {
        return TieredPayBandAllocationPolicy.ClassifyTieredPayBandAllocation(item.EffectiveExposure());
    }
}
