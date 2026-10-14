namespace Acme.Workforce.Contractors.Offboarding.OffboardingSegment;

using Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

/// <summary>Functions of adaptive offboarding segment from bundle.</summary>
public static class AdaptiveOffboardingSegmentFromBundle
{
    public static Dictionary<string, double> SummarizeTimesheetBundlesForSegment(List<TimesheetBundle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<AdaptiveOffboardingSegment> DeriveAdaptiveOffboardingSegmentsFromBundle(List<TimesheetBundle> items)
    {
        var outValue = new List<AdaptiveOffboardingSegment>();
        foreach (var item in items)
        {
            var derived = AdaptiveOffboardingSegment.NewAdaptiveOffboardingSegment(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeAdaptiveOffboardingSegment(AdaptiveOffboardingSegment item)
    {
        return TimesheetClaimPolicy.NormalizeTimesheetClaimCode(item.Code);
    }
}
