namespace Acme.Workforce.Absence.LeaveRequest.Ledger;

using Acme.Workforce.Absence.Accrual.OverrideItem;

/// <summary>Functions of leave request target from plan.</summary>
public static class LeaveRequestTargetFromPlan
{
    public static Dictionary<string, double> SummarizePrimaryAccrualPlansForTarget(List<PrimaryAccrualPlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<LeaveRequestTarget> DeriveLeaveRequestTargetsFromPlan(List<PrimaryAccrualPlan> items)
    {
        var outValue = new List<LeaveRequestTarget>();
        foreach (var item in items)
        {
            var derived = LeaveRequestTarget.NewLeaveRequestTarget(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeLeaveRequestTarget(LeaveRequestTarget item)
    {
        return RollingAccrualWindowPolicy.NormalizeRollingAccrualWindowCode(item.Code);
    }
}
