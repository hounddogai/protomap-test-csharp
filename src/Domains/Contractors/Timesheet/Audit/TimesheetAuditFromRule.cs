namespace Acme.Workforce.Contractors.Timesheet.Audit;

using Acme.Workforce.Contractors.Rate.Preference;

/// <summary>Functions of timesheet audit from rule.</summary>
public static class TimesheetAuditFromRule
{
    public static Dictionary<string, double> SummarizeRateRulesForAudit(List<RateRule> items)
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

    public static List<TimesheetAudit> DeriveTimesheetAuditsFromRule(List<RateRule> items)
    {
        var outValue = new List<TimesheetAudit>();
        foreach (var item in items)
        {
            var derived = TimesheetAudit.NewTimesheetAudit(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SummarizeTimesheetAudit(TimesheetAudit item)
    {
        return BatchedRatePreferencePolicy.BatchedRatePreferenceWeight(9, true);
    }
}
