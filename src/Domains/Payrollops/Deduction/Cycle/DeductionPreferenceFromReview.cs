namespace Acme.Workforce.Payrollops.Deduction.Cycle;

using Acme.Workforce.Payrollops.RetroPay.Checkpoint;

/// <summary>Functions of deduction preference from review.</summary>
public static class DeductionPreferenceFromReview
{
    public static Dictionary<string, double> SummarizeSeasonalRetroPayReviewsForPreference(List<SeasonalRetroPayReview> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<DeductionPreference> DeriveDeductionPreferencesFromReview(List<SeasonalRetroPayReview> items)
    {
        var outValue = new List<DeductionPreference>();
        foreach (var item in items)
        {
            var derived = DeductionPreference.NewDeductionPreference(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyDeductionPreference(DeductionPreference item)
    {
        return RetroPayCheckpointPolicy.NormalizeRetroPayCheckpointCode(item.Code);
    }
}
