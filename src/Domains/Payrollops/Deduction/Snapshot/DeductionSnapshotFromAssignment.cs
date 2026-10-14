namespace Acme.Workforce.Payrollops.Deduction.Snapshot;

using Acme.Workforce.Payrollops.Payslip.Allocation;

/// <summary>Functions of deduction snapshot from assignment.</summary>
public static class DeductionSnapshotFromAssignment
{
    public static Dictionary<string, double> SummarizeRegionalPayslipAssignmentsForSnapshot(List<RegionalPayslipAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<DeductionSnapshot> DeriveDeductionSnapshotsFromAssignment(List<RegionalPayslipAssignment> items)
    {
        var outValue = new List<DeductionSnapshot>();
        foreach (var item in items)
        {
            var derived = DeductionSnapshot.NewDeductionSnapshot(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SummarizeDeductionSnapshot(DeductionSnapshot item)
    {
        return PayslipForecastPolicy.PayslipForecastWeight(2, true);
    }
}
