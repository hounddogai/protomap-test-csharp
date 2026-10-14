namespace Acme.Workforce.Absence.LeaveRequest.Digest;

using Acme.Workforce.Absence.Holiday.Proposal;

/// <summary>Functions of leave request plan from adjustment.</summary>
public static class LeaveRequestPlanFromAdjustment
{
    public static Dictionary<string, double> SummarizeHolidayAdjustmentsForPlan(List<HolidayAdjustment> items)
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

    public static List<LeaveRequestPlan> DeriveLeaveRequestPlansFromAdjustment(List<HolidayAdjustment> items)
    {
        var outValue = new List<LeaveRequestPlan>();
        foreach (var item in items)
        {
            var derived = LeaveRequestPlan.NewLeaveRequestPlan(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankLeaveRequestPlan(LeaveRequestPlan item)
    {
        return HolidayProposalPolicy.NormalizeHolidayProposalCode(item.Code);
    }
}
