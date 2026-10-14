namespace Acme.Workforce.Absence.ReturnPlan.Entry;

using Acme.Workforce.Absence.ReturnPlan.Segment;

/// <summary>Functions of adaptive return plan entry from segment.</summary>
public static class AdaptiveReturnPlanEntryFromSegment
{
    public static Dictionary<string, double> SummarizeReturnPlanSegmentsForEntry(List<ReturnPlanSegment> items)
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

    public static List<AdaptiveReturnPlanEntry> DeriveAdaptiveReturnPlanEntriesFromSegment(List<ReturnPlanSegment> items)
    {
        var outValue = new List<AdaptiveReturnPlanEntry>();
        foreach (var item in items)
        {
            var derived = AdaptiveReturnPlanEntry.NewAdaptiveReturnPlanEntry(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeAdaptiveReturnPlanEntry(AdaptiveReturnPlanEntry item)
    {
        return ReturnPlanSegmentPolicy.NormalizeReturnPlanSegmentCode(item.Code);
    }
}
