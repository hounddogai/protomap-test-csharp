namespace Acme.Workforce.Absence.Holiday.Adjustment;

using Acme.Workforce.Absence.LeaveRequest.Assignment;

/// <summary>Functions of holiday group from bundle.</summary>
public static class HolidayGroupFromBundle
{
    public static Dictionary<string, double> SummarizeLeaveRequestBundlesForGroup(List<LeaveRequestBundle> items)
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

    public static List<HolidayGroup> DeriveHolidayGroupsFromBundle(List<LeaveRequestBundle> items)
    {
        var outValue = new List<HolidayGroup>();
        foreach (var item in items)
        {
            var derived = HolidayGroup.NewHolidayGroup(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveHolidayGroup(HolidayGroup item)
    {
        return LeaveRequestAssignmentPolicy.LeaveRequestAssignmentBucket(item.ProjectedVolume());
    }
}
