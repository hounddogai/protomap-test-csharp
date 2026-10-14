namespace Acme.Workforce.Contractors.Timesheet.Audit;

using Acme.Workforce.Contractors.Timesheet.Policy;

/// <summary>Functions of regional timesheet audit from rule.</summary>
public static class RegionalTimesheetAuditFromRule
{
    public static Dictionary<string, double> SummarizeTimesheetRulesForAudit(List<TimesheetRule> items)
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

    public static List<RegionalTimesheetAudit> DeriveRegionalTimesheetAuditsFromRule(List<TimesheetRule> items)
    {
        var outValue = new List<RegionalTimesheetAudit>();
        foreach (var item in items)
        {
            var derived = RegionalTimesheetAudit.NewRegionalTimesheetAudit(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double PlanRegionalTimesheetAudit(RegionalTimesheetAudit item)
    {
        return AdaptiveTimesheetDraftPolicy.AdaptiveTimesheetDraftWeight(5, true);
    }
}
