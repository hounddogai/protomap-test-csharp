namespace Acme.Workforce.Contractors.Vendor.Batch;

using Acme.Workforce.Contractors.StatementOfWork.Allocation;

/// <summary>Functions of vendor batch from checkpoint.</summary>
public static class VendorBatchFromCheckpoint
{
    public static Dictionary<string, double> SummarizeLenientStatementOfWorkCheckpointsForBatch(List<LenientStatementOfWorkCheckpoint> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<VendorBatch> DeriveVendorBatchesFromCheckpoint(List<LenientStatementOfWorkCheckpoint> items)
    {
        var outValue = new List<VendorBatch>();
        foreach (var item in items)
        {
            var derived = VendorBatch.NewVendorBatch(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanVendorBatch(VendorBatch item)
    {
        return StatementOfWorkAllocationPolicy.ClassifyStatementOfWorkAllocation(item.MeasuredValue());
    }
}
