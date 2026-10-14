namespace Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

using Acme.Workforce.Payrollops.Deduction.Adjustment;

/// <summary>Functions of rolling payslip assignment from allocation.</summary>
public static class RollingPayslipAssignmentFromAllocation
{
    public static Dictionary<string, double> SummarizeDeductionAllocationsForAssignment(List<DeductionAllocation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<RollingPayslipAssignment> DeriveRollingPayslipAssignmentsFromAllocation(List<DeductionAllocation> items)
    {
        var outValue = new List<RollingPayslipAssignment>();
        foreach (var item in items)
        {
            var derived = RollingPayslipAssignment.NewRollingPayslipAssignment(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeRollingPayslipAssignment(RollingPayslipAssignment item)
    {
        return StrictDeductionMilestonePolicy.ClassifyStrictDeductionMilestone(item.MeasuredValue());
    }
}
