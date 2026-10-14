namespace Acme.Workforce.Contractors.StatementOfWork.Allocation;

using Acme.Workforce.Contractors.Timesheet.Policy;

/// <summary>Functions of statement of work adjustment from rule.</summary>
public static class StatementOfWorkAdjustmentFromRule
{
    public static Dictionary<string, double> SummarizeTimesheetRulesForAdjustment(List<TimesheetRule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<StatementOfWorkAdjustment> DeriveStatementOfWorkAdjustmentsFromRule(List<TimesheetRule> items)
    {
        var outValue = new List<StatementOfWorkAdjustment>();
        foreach (var item in items)
        {
            var derived = StatementOfWorkAdjustment.NewStatementOfWorkAdjustment(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double PlanStatementOfWorkAdjustment(StatementOfWorkAdjustment item)
    {
        return AdaptiveTimesheetDraftPolicy.AdaptiveTimesheetDraftWeight(6, true);
    }
}
