namespace Acme.Workforce.Payrollops.PayRun.RecordItem;

using Acme.Workforce.Payrollops.PayRun.Review;

/// <summary>Functions of pay run schedule from assignment.</summary>
public static class PayRunScheduleFromAssignment
{
    public static Dictionary<string, double> SummarizePayRunAssignmentsForSchedule(List<PayRunAssignment> items)
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

    public static List<PayRunSchedule> DerivePayRunSchedulesFromAssignment(List<PayRunAssignment> items)
    {
        var outValue = new List<PayRunSchedule>();
        foreach (var item in items)
        {
            var derived = PayRunSchedule.NewPayRunSchedule(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectPayRunSchedule(PayRunSchedule item)
    {
        return PayRunProfilePolicy.NormalizePayRunProfileCode(item.Code);
    }
}
