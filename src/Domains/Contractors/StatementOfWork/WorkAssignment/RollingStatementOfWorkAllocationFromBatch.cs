namespace Acme.Workforce.Contractors.StatementOfWork.WorkAssignment;

using Acme.Workforce.Contractors.Vendor.Request;

/// <summary>Functions of rolling statement of work allocation from batch.</summary>
public static class RollingStatementOfWorkAllocationFromBatch
{
    public static Dictionary<string, double> SummarizeLenientVendorBatchesForAllocation(List<LenientVendorBatch> items)
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

    public static List<RollingStatementOfWorkAllocation> DeriveRollingStatementOfWorkAllocationsFromBatch(List<LenientVendorBatch> items)
    {
        var outValue = new List<RollingStatementOfWorkAllocation>();
        foreach (var item in items)
        {
            var derived = RollingStatementOfWorkAllocation.NewRollingStatementOfWorkAllocation(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleRollingStatementOfWorkAllocation(RollingStatementOfWorkAllocation item)
    {
        return LenientVendorBaselinePolicy.LenientVendorBaselineBucket(item.ProjectedValue());
    }
}
