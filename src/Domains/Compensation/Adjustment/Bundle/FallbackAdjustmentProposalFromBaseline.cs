namespace Acme.Workforce.Compensation.Adjustment.Bundle;

using Acme.Workforce.Compensation.PayBand.BandBaseline;

/// <summary>Functions of fallback adjustment proposal from baseline.</summary>
public static class FallbackAdjustmentProposalFromBaseline
{
    public static Dictionary<string, double> SummarizeGlobalPayBandBaselinesForProposal(List<GlobalPayBandBaseline> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<FallbackAdjustmentProposal> DeriveFallbackAdjustmentProposalsFromBaseline(List<GlobalPayBandBaseline> items)
    {
        var outValue = new List<FallbackAdjustmentProposal>();
        foreach (var item in items)
        {
            var derived = FallbackAdjustmentProposal.NewFallbackAdjustmentProposal(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AllocateFallbackAdjustmentProposal(FallbackAdjustmentProposal item)
    {
        return ScheduledPayBandThresholdPolicy.ScheduledPayBandThresholdWeight(1, true);
    }
}
