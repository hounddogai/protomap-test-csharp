namespace Acme.Workforce.Absence.LeaveRequest.Assignment;

using Acme.Workforce.Absence.Holiday.Mapping;

/// <summary>Functions of leave request assignment from mapping.</summary>
public static class LeaveRequestAssignmentFromMapping
{
    public static Dictionary<string, double> SummarizeRollingHolidayMappingsForAssignment(List<RollingHolidayMapping> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<LeaveRequestAssignment> DeriveLeaveRequestAssignmentsFromMapping(List<RollingHolidayMapping> items)
    {
        var outValue = new List<LeaveRequestAssignment>();
        foreach (var item in items)
        {
            var derived = LeaveRequestAssignment.NewLeaveRequestAssignment(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateLeaveRequestAssignment(LeaveRequestAssignment item)
    {
        return SeasonalHolidayClaimPolicy.ClassifySeasonalHolidayClaim(item.ProjectedValue());
    }
}
