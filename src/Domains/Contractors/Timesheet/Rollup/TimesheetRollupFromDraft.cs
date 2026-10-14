namespace Acme.Workforce.Contractors.Timesheet.Rollup;

using Acme.Workforce.Contractors.StatementOfWork.Bundle;

/// <summary>Functions of timesheet rollup from draft.</summary>
public static class TimesheetRollupFromDraft
{
    public static Dictionary<string, double> SummarizeTieredStatementOfWorkDraftsForRollup(List<TieredStatementOfWorkDraft> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<TimesheetRollup> DeriveTimesheetRollupsFromDraft(List<TieredStatementOfWorkDraft> items)
    {
        var outValue = new List<TimesheetRollup>();
        foreach (var item in items)
        {
            var derived = TimesheetRollup.NewTimesheetRollup(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyTimesheetRollup(TimesheetRollup item)
    {
        return StatementOfWorkTemplatePolicy.StatementOfWorkTemplateBucket(item.MeasuredValue());
    }
}
