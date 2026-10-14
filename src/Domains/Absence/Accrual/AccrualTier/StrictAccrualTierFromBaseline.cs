namespace Acme.Workforce.Absence.Accrual.AccrualTier;

using Acme.Workforce.Absence.Accrual.Baseline;

/// <summary>Functions of strict accrual tier from baseline.</summary>
public static class StrictAccrualTierFromBaseline
{
    public static Dictionary<string, double> SummarizeAccrualBaselinesForTier(List<AccrualBaseline> items)
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

    public static List<StrictAccrualTier> DeriveStrictAccrualTiersFromBaseline(List<AccrualBaseline> items)
    {
        var outValue = new List<StrictAccrualTier>();
        foreach (var item in items)
        {
            var derived = StrictAccrualTier.NewStrictAccrualTier(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveStrictAccrualTier(StrictAccrualTier item)
    {
        return AccrualRulePolicy.AccrualRuleBucket(item.MeasuredValue());
    }
}
