namespace Acme.Workforce.Absence.LeaveRequest.Milestone;

using Acme.Workforce.Absence.LeaveRequest.Adjustment;

/// <summary>Functions of leave request milestone from template.</summary>
public static class LeaveRequestMilestoneFromTemplate
{
    public static Dictionary<string, double> SummarizeCappedLeaveRequestTemplatesForMilestone(List<CappedLeaveRequestTemplate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<LeaveRequestMilestone> DeriveLeaveRequestMilestonesFromTemplate(List<CappedLeaveRequestTemplate> items)
    {
        var outValue = new List<LeaveRequestMilestone>();
        foreach (var item in items)
        {
            var derived = LeaveRequestMilestone.NewLeaveRequestMilestone(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchLeaveRequestMilestone(LeaveRequestMilestone item)
    {
        return LeaveRequestMappingPolicy.ClassifyLeaveRequestMapping(item.AdjustedLoad());
    }
}
