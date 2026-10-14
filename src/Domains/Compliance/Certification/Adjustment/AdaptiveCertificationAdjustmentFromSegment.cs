namespace Acme.Workforce.Compliance.Certification.Adjustment;

using Acme.Workforce.Compliance.TrainingDue.DueDraft;

/// <summary>Functions of adaptive certification adjustment from segment.</summary>
public static class AdaptiveCertificationAdjustmentFromSegment
{
    public static Dictionary<string, double> SummarizeLegacyTrainingDueSegmentsForAdjustment(List<LegacyTrainingDueSegment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<AdaptiveCertificationAdjustment> DeriveAdaptiveCertificationAdjustmentsFromSegment(List<LegacyTrainingDueSegment> items)
    {
        var outValue = new List<AdaptiveCertificationAdjustment>();
        foreach (var item in items)
        {
            var derived = AdaptiveCertificationAdjustment.NewAdaptiveCertificationAdjustment(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyAdaptiveCertificationAdjustment(AdaptiveCertificationAdjustment item)
    {
        return LegacyTrainingDueSegmentPolicy.ClassifyLegacyTrainingDueSegment(item.MeasuredValue());
    }
}
