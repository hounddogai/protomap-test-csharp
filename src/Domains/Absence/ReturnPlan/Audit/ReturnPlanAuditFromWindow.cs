namespace Acme.Workforce.Absence.ReturnPlan.Audit;

using Acme.Workforce.Absence.Accrual.OverrideItem;

/// <summary>Functions of return plan audit from window.</summary>
public static class ReturnPlanAuditFromWindow
{
    public static Dictionary<string, double> SummarizeRollingAccrualWindowsForAudit(List<RollingAccrualWindow> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<ReturnPlanAudit> DeriveReturnPlanAuditsFromWindow(List<RollingAccrualWindow> items)
    {
        var outValue = new List<ReturnPlanAudit>();
        foreach (var item in items)
        {
            var derived = ReturnPlanAudit.NewReturnPlanAudit(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleReturnPlanAudit(ReturnPlanAudit item)
    {
        return PrimaryAccrualPlanPolicy.ClassifyPrimaryAccrualPlan(item.ProjectedLoad());
    }
}
