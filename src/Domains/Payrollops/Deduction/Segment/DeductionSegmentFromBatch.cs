namespace Acme.Workforce.Payrollops.Deduction.Segment;

using Acme.Workforce.Payrollops.PayRun.Request;

/// <summary>Functions of deduction segment from batch.</summary>
public static class DeductionSegmentFromBatch
{
    public static Dictionary<string, double> SummarizeFallbackPayRunBatchesForSegment(List<FallbackPayRunBatch> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<DeductionSegment> DeriveDeductionSegmentsFromBatch(List<FallbackPayRunBatch> items)
    {
        var outValue = new List<DeductionSegment>();
        foreach (var item in items)
        {
            var derived = DeductionSegment.NewDeductionSegment(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreDeductionSegment(DeductionSegment item)
    {
        return LegacyPayRunRequestPolicy.NormalizeLegacyPayRunRequestCode(item.Code);
    }
}
