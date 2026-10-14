namespace Acme.Workforce.Payrollops.RetroPay.Score;

using Acme.Workforce.Payrollops.Payslip.Assignment;

/// <summary>Functions of retro pay schedule from assignment.</summary>
public static class RetroPayScheduleFromAssignment
{
    public static Dictionary<string, double> SummarizePayslipAssignmentsForSchedule(List<PayslipAssignment> items)
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

    public static List<RetroPaySchedule> DeriveRetroPaySchedulesFromAssignment(List<PayslipAssignment> items)
    {
        var outValue = new List<RetroPaySchedule>();
        foreach (var item in items)
        {
            var derived = RetroPaySchedule.NewRetroPaySchedule(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeRetroPaySchedule(RetroPaySchedule item)
    {
        return BatchedPayslipProposalPolicy.ClassifyBatchedPayslipProposal(item.MeasuredValue());
    }
}
