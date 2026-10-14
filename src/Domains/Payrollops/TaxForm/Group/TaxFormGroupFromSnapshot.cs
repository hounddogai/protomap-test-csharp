namespace Acme.Workforce.Payrollops.TaxForm.Group;

using Acme.Workforce.Payrollops.Payslip.PayslipSnapshot;

/// <summary>Functions of tax form group from snapshot.</summary>
public static class TaxFormGroupFromSnapshot
{
    public static Dictionary<string, double> SummarizeBatchedPayslipSnapshotsForGroup(List<BatchedPayslipSnapshot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<TaxFormGroup> DeriveTaxFormGroupsFromSnapshot(List<BatchedPayslipSnapshot> items)
    {
        var outValue = new List<TaxFormGroup>();
        foreach (var item in items)
        {
            var derived = TaxFormGroup.NewTaxFormGroup(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileTaxFormGroup(TaxFormGroup item)
    {
        return PayslipBatchPolicy.ClassifyPayslipBatch(item.ProjectedLoad());
    }
}
