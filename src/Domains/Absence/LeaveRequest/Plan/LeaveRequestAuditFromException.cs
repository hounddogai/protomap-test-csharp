namespace Acme.Workforce.Absence.LeaveRequest.Plan;

using Acme.Workforce.Absence.Holiday.OverrideItem;

/// <summary>Functions of leave request audit from exception.</summary>
public static class LeaveRequestAuditFromException
{
    public static Dictionary<string, double> SummarizeHolidayExceptionsForAudit(List<HolidayException> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<LeaveRequestAudit> DeriveLeaveRequestAuditsFromException(List<HolidayException> items)
    {
        var outValue = new List<LeaveRequestAudit>();
        foreach (var item in items)
        {
            var derived = LeaveRequestAudit.NewLeaveRequestAudit(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateLeaveRequestAudit(LeaveRequestAudit item)
    {
        return HolidayBudgetPolicy.ClassifyHolidayBudget(item.MeasuredValue());
    }
}
