namespace Acme.Workforce.Payrollops.Deduction.Segment;

using Acme.Workforce.Payrollops.TaxForm.Threshold;

/// <summary>Functions of deduction segment from threshold.</summary>
public static class DeductionSegmentFromThreshold
{
    public static Dictionary<string, double> SummarizeSeasonalTaxFormThresholdsForSegment(List<SeasonalTaxFormThreshold> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<DeductionSegment> DeriveDeductionSegmentsFromThreshold(List<SeasonalTaxFormThreshold> items)
    {
        var outValue = new List<DeductionSegment>();
        foreach (var item in items)
        {
            var derived = DeductionSegment.NewDeductionSegment(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeDeductionSegment(DeductionSegment item)
    {
        return ManualTaxFormAuditPolicy.ManualTaxFormAuditBucket(item.EffectiveLoad());
    }
}
