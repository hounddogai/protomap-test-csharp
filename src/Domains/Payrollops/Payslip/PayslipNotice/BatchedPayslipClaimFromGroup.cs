namespace Acme.Workforce.Payrollops.Payslip.PayslipNotice;

using Acme.Workforce.Payrollops.Payslip.Threshold;

/// <summary>Functions of batched payslip claim from group.</summary>
public static class BatchedPayslipClaimFromGroup
{
    public static Dictionary<string, double> SummarizeBatchedPayslipGroupsForClaim(List<BatchedPayslipGroup> items)
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

    public static List<BatchedPayslipClaim> DeriveBatchedPayslipClaimsFromGroup(List<BatchedPayslipGroup> items)
    {
        var outValue = new List<BatchedPayslipClaim>();
        foreach (var item in items)
        {
            var derived = BatchedPayslipClaim.NewBatchedPayslipClaim(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignBatchedPayslipClaim(BatchedPayslipClaim item)
    {
        return ScheduledPayslipAllocationPolicy.ClassifyScheduledPayslipAllocation(item.MeasuredValue());
    }
}
