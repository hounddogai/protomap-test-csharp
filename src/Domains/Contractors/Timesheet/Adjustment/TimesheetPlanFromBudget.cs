namespace Acme.Workforce.Contractors.Timesheet.Adjustment;

using Acme.Workforce.Contractors.Engagement.Tier;

/// <summary>Functions of timesheet plan from budget.</summary>
public static class TimesheetPlanFromBudget
{
    public static Dictionary<string, double> SummarizeRegionalEngagementBudgetsForPlan(List<RegionalEngagementBudget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<TimesheetPlan> DeriveTimesheetPlansFromBudget(List<RegionalEngagementBudget> items)
    {
        var outValue = new List<TimesheetPlan>();
        foreach (var item in items)
        {
            var derived = TimesheetPlan.NewTimesheetPlan(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchTimesheetPlan(TimesheetPlan item)
    {
        return EngagementSummaryPolicy.ClassifyEngagementSummary(item.WeightedExposure());
    }
}
