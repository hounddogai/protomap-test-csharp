namespace Acme.Workforce.Absence.Policy.Segment;

using Acme.Workforce.Absence.Holiday.Allocation;

/// <summary>Functions of policy segment from allocation.</summary>
public static class PolicySegmentFromAllocation
{
    public static Dictionary<string, double> SummarizeHolidayAllocationsForSegment(List<HolidayAllocation> items)
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

    public static List<PolicySegment> DerivePolicySegmentsFromAllocation(List<HolidayAllocation> items)
    {
        var outValue = new List<PolicySegment>();
        foreach (var item in items)
        {
            var derived = PolicySegment.NewPolicySegment(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MergePolicySegment(PolicySegment item)
    {
        return SeasonalHolidayRollupPolicy.SeasonalHolidayRollupWeight(3, true);
    }
}
