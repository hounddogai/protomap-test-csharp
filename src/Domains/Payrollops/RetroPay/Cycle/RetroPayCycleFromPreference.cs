namespace Acme.Workforce.Payrollops.RetroPay.Cycle;

using Acme.Workforce.Payrollops.Garnishment.Preference;

/// <summary>Functions of retro pay cycle from preference.</summary>
public static class RetroPayCycleFromPreference
{
    public static Dictionary<string, double> SummarizeGarnishmentPreferencesForCycle(List<GarnishmentPreference> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<RetroPayCycle> DeriveRetroPayCyclesFromPreference(List<GarnishmentPreference> items)
    {
        var outValue = new List<RetroPayCycle>();
        foreach (var item in items)
        {
            var derived = RetroPayCycle.NewRetroPayCycle(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankRetroPayCycle(RetroPayCycle item)
    {
        return GarnishmentPreferencePolicy.GarnishmentPreferenceBucket(item.EffectiveTotal());
    }
}
