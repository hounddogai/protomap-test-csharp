namespace Acme.Workforce.Absence.LeaveRequest.Batch;

using Acme.Workforce.Absence.LeaveRequest.Schedule;

/// <summary>Functions of seasonal leave request rule from item.</summary>
public static class SeasonalLeaveRequestRuleFromItem
{
    public static Dictionary<string, double> SummarizeBatchedLeaveRequestItemsForRule(List<BatchedLeaveRequestItem> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<SeasonalLeaveRequestRule> DeriveSeasonalLeaveRequestRulesFromItem(List<BatchedLeaveRequestItem> items)
    {
        var outValue = new List<SeasonalLeaveRequestRule>();
        foreach (var item in items)
        {
            var derived = SeasonalLeaveRequestRule.NewSeasonalLeaveRequestRule(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeSeasonalLeaveRequestRule(SeasonalLeaveRequestRule item)
    {
        return LeaveRequestSchedulePolicy.ClassifyLeaveRequestSchedule(item.MeasuredValue());
    }
}
