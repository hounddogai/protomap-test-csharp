namespace Acme.Workforce.Payrollops.Payslip.Snapshot;

using Acme.Workforce.Payrollops.RetroPay.Assignment;

/// <summary>Functions of global payslip notice from assignment.</summary>
public static class GlobalPayslipNoticeFromAssignment
{
    public static Dictionary<string, double> SummarizeRetroPayAssignmentsForNotice(List<RetroPayAssignment> items)
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

    public static List<GlobalPayslipNotice> DeriveGlobalPayslipNoticesFromAssignment(List<RetroPayAssignment> items)
    {
        var outValue = new List<GlobalPayslipNotice>();
        foreach (var item in items)
        {
            var derived = GlobalPayslipNotice.NewGlobalPayslipNotice(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleGlobalPayslipNotice(GlobalPayslipNotice item)
    {
        return RetroPayAssignmentPolicy.RetroPayAssignmentWeight(1, true);
    }
}
