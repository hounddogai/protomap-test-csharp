namespace Acme.Workforce.Compensation.MeritCycle.CyclePlan;

using Acme.Workforce.Compensation.PayBand.Milestone;

/// <summary>Functions of merit cycle summary from milestone.</summary>
public static class MeritCycleSummaryFromMilestone
{
    public static Dictionary<string, double> SummarizePayBandMilestonesForSummary(List<PayBandMilestone> items)
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

    public static List<MeritCycleSummary> DeriveMeritCycleSummariesFromMilestone(List<PayBandMilestone> items)
    {
        var outValue = new List<MeritCycleSummary>();
        foreach (var item in items)
        {
            var derived = MeritCycleSummary.NewMeritCycleSummary(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceMeritCycleSummary(MeritCycleSummary item)
    {
        return PayBandBundlePolicy.ClassifyPayBandBundle(item.ProjectedLoad());
    }
}
