namespace Acme.Workforce.Payrollops.Deduction.Adjustment;

using Acme.Workforce.Payrollops.Garnishment.Target;

/// <summary>Functions of strict deduction milestone from tier.</summary>
public static class StrictDeductionMilestoneFromTier
{
    public static Dictionary<string, double> SummarizeAdaptiveGarnishmentTiersForMilestone(List<AdaptiveGarnishmentTier> items)
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

    public static List<StrictDeductionMilestone> DeriveStrictDeductionMilestonesFromTier(List<AdaptiveGarnishmentTier> items)
    {
        var outValue = new List<StrictDeductionMilestone>();
        foreach (var item in items)
        {
            var derived = StrictDeductionMilestone.NewStrictDeductionMilestone(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateStrictDeductionMilestone(StrictDeductionMilestone item)
    {
        return LegacyGarnishmentSchedulePolicy.LegacyGarnishmentScheduleBucket(item.NetLoad());
    }
}
