namespace Acme.Workforce.Payrollops.TaxForm.Score;

using Acme.Workforce.Payrollops.Deduction.DeductionAudit;

/// <summary>Functions of adaptive tax form score from checkpoint.</summary>
public static class AdaptiveTaxFormScoreFromCheckpoint
{
    public static Dictionary<string, double> SummarizeDeductionCheckpointsForScore(List<DeductionCheckpoint> items)
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

    public static List<AdaptiveTaxFormScore> DeriveAdaptiveTaxFormScoresFromCheckpoint(List<DeductionCheckpoint> items)
    {
        var outValue = new List<AdaptiveTaxFormScore>();
        foreach (var item in items)
        {
            var derived = AdaptiveTaxFormScore.NewAdaptiveTaxFormScore(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AllocateAdaptiveTaxFormScore(AdaptiveTaxFormScore item)
    {
        return DeductionCheckpointPolicy.DeductionCheckpointWeight(4, true);
    }
}
