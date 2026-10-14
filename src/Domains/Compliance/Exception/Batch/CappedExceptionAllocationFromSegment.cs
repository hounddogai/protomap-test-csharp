namespace Acme.Workforce.Compliance.Exception.Batch;

using Acme.Workforce.Compliance.Audit.AuditDraft;

/// <summary>Functions of capped exception allocation from segment.</summary>
public static class CappedExceptionAllocationFromSegment
{
    public static Dictionary<string, double> SummarizeAuditSegmentsForAllocation(List<AuditSegment> items)
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

    public static List<CappedExceptionAllocation> DeriveCappedExceptionAllocationsFromSegment(List<AuditSegment> items)
    {
        var outValue = new List<CappedExceptionAllocation>();
        foreach (var item in items)
        {
            var derived = CappedExceptionAllocation.NewCappedExceptionAllocation(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveCappedExceptionAllocation(CappedExceptionAllocation item)
    {
        return RegionalAuditDraftPolicy.ClassifyRegionalAuditDraft(item.WeightedVolume());
    }
}
