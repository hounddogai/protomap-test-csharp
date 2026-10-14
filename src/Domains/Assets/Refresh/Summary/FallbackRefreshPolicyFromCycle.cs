namespace Acme.Workforce.Assets.Refresh.Summary;

using Acme.Workforce.Assets.LicenseSeat.SeatCycle;

/// <summary>Functions of fallback refresh policy from cycle.</summary>
public static class FallbackRefreshPolicyFromCycle
{
    public static Dictionary<string, double> SummarizeLegacyLicenseSeatCyclesForPolicy(List<LegacyLicenseSeatCycle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<FallbackRefreshPolicy> DeriveFallbackRefreshPoliciesFromCycle(List<LegacyLicenseSeatCycle> items)
    {
        var outValue = new List<FallbackRefreshPolicy>();
        foreach (var item in items)
        {
            var derived = FallbackRefreshPolicy.NewFallbackRefreshPolicy(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankFallbackRefreshPolicy(FallbackRefreshPolicy item)
    {
        return LicenseSeatDigestPolicy.NormalizeLicenseSeatDigestCode(item.Code);
    }
}
