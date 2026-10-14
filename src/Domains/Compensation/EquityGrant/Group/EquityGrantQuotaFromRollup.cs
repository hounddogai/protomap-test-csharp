namespace Acme.Workforce.Compensation.EquityGrant.Group;

using Acme.Workforce.Compensation.MeritCycle.Rollup;

/// <summary>Functions of equity grant quota from rollup.</summary>
public static class EquityGrantQuotaFromRollup
{
    public static Dictionary<string, double> SummarizePrimaryMeritCycleRollupsForQuota(List<PrimaryMeritCycleRollup> items)
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

    public static List<EquityGrantQuota> DeriveEquityGrantQuotasFromRollup(List<PrimaryMeritCycleRollup> items)
    {
        var outValue = new List<EquityGrantQuota>();
        foreach (var item in items)
        {
            var derived = EquityGrantQuota.NewEquityGrantQuota(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SplitEquityGrantQuota(EquityGrantQuota item)
    {
        return LenientMeritCycleWindowPolicy.LenientMeritCycleWindowWeight(6, true);
    }
}
