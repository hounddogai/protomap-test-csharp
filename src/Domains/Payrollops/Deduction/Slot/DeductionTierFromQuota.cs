namespace Acme.Workforce.Payrollops.Deduction.Slot;

using Acme.Workforce.Payrollops.Deduction.Quota;

/// <summary>Functions of deduction tier from quota.</summary>
public static class DeductionTierFromQuota
{
    public static Dictionary<string, double> SummarizeDeductionQuotasForTier(List<DeductionQuota> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<DeductionTier> DeriveDeductionTiersFromQuota(List<DeductionQuota> items)
    {
        var outValue = new List<DeductionTier>();
        foreach (var item in items)
        {
            var derived = DeductionTier.NewDeductionTier(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ScoreDeductionTier(DeductionTier item)
    {
        return LenientDeductionEntryPolicy.LenientDeductionEntryWeight(1, true);
    }
}
