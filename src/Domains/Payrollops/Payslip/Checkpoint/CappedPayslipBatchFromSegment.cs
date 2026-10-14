namespace Acme.Workforce.Payrollops.Payslip.Checkpoint;

using Acme.Workforce.Payrollops.Deduction.Segment;

/// <summary>Functions of capped payslip batch from segment.</summary>
public static class CappedPayslipBatchFromSegment
{
    public static Dictionary<string, double> SummarizeStrictDeductionSegmentsForBatch(List<StrictDeductionSegment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<CappedPayslipBatch> DeriveCappedPayslipBatchesFromSegment(List<StrictDeductionSegment> items)
    {
        var outValue = new List<CappedPayslipBatch>();
        foreach (var item in items)
        {
            var derived = CappedPayslipBatch.NewCappedPayslipBatch(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveCappedPayslipBatch(CappedPayslipBatch item)
    {
        return DeductionSegmentPolicy.DeductionSegmentBucket(item.WeightedValue());
    }
}
