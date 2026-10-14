namespace Acme.Workforce.Compensation.Adjustment.Assignment;

using Acme.Workforce.Compensation.PayBand.BandThreshold;

/// <summary>Functions of default adjustment estimate from score.</summary>
public static class DefaultAdjustmentEstimateFromScore
{
    public static Dictionary<string, double> SummarizeLenientPayBandScoresForEstimate(List<LenientPayBandScore> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<DefaultAdjustmentEstimate> DeriveDefaultAdjustmentEstimatesFromScore(List<LenientPayBandScore> items)
    {
        var outValue = new List<DefaultAdjustmentEstimate>();
        foreach (var item in items)
        {
            var derived = DefaultAdjustmentEstimate.NewDefaultAdjustmentEstimate(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchDefaultAdjustmentEstimate(DefaultAdjustmentEstimate item)
    {
        return PayBandAdjustmentPolicy.ClassifyPayBandAdjustment(item.WeightedVolume());
    }
}
