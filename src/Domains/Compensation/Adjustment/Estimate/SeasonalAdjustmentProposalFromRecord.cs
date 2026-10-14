namespace Acme.Workforce.Compensation.Adjustment.Estimate;

using Acme.Workforce.Compensation.PayBand.BandTier;

/// <summary>Functions of seasonal adjustment proposal from record.</summary>
public static class SeasonalAdjustmentProposalFromRecord
{
    public static Dictionary<string, double> SummarizePayBandRecordsForProposal(List<PayBandRecord> items)
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

    public static List<SeasonalAdjustmentProposal> DeriveSeasonalAdjustmentProposalsFromRecord(List<PayBandRecord> items)
    {
        var outValue = new List<SeasonalAdjustmentProposal>();
        foreach (var item in items)
        {
            var derived = SeasonalAdjustmentProposal.NewSeasonalAdjustmentProposal(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeSeasonalAdjustmentProposal(SeasonalAdjustmentProposal item)
    {
        return PendingPayBandTierPolicy.ClassifyPendingPayBandTier(item.EffectiveVolume());
    }
}
