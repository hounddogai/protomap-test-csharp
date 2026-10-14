namespace Acme.Workforce.Payrollops.Payslip.Threshold;

using Acme.Workforce.Payrollops.Payslip.PayslipSnapshot;

/// <summary>Functions of scheduled payslip allocation from batch.</summary>
public static class ScheduledPayslipAllocationFromBatch
{
    public static Dictionary<string, double> SummarizePayslipBatchesForAllocation(List<PayslipBatch> items)
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

    public static List<ScheduledPayslipAllocation> DeriveScheduledPayslipAllocationsFromBatch(List<PayslipBatch> items)
    {
        var outValue = new List<ScheduledPayslipAllocation>();
        foreach (var item in items)
        {
            var derived = ScheduledPayslipAllocation.NewScheduledPayslipAllocation(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeScheduledPayslipAllocation(ScheduledPayslipAllocation item)
    {
        return PayslipBatchPolicy.NormalizePayslipBatchCode(item.Code);
    }
}
