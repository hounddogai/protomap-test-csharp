namespace Acme.Workforce.Payrollops.RetroPay.Quota;

using Acme.Workforce.Payrollops.Garnishment.Score;

/// <summary>Functions of lenient retro pay rollup from plan.</summary>
public static class LenientRetroPayRollupFromPlan
{
    public static Dictionary<string, double> SummarizeScheduledGarnishmentPlansForRollup(List<ScheduledGarnishmentPlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<LenientRetroPayRollup> DeriveLenientRetroPayRollupsFromPlan(List<ScheduledGarnishmentPlan> items)
    {
        var outValue = new List<LenientRetroPayRollup>();
        foreach (var item in items)
        {
            var derived = LenientRetroPayRollup.NewLenientRetroPayRollup(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitLenientRetroPayRollup(LenientRetroPayRollup item)
    {
        return GarnishmentScorePolicy.ClassifyGarnishmentScore(item.EffectiveExposure());
    }
}
