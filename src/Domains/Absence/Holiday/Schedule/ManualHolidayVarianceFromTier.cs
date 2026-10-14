namespace Acme.Workforce.Absence.Holiday.Schedule;

using Acme.Workforce.Absence.Accrual.AccrualTier;

/// <summary>Functions of manual holiday variance from tier.</summary>
public static class ManualHolidayVarianceFromTier
{
    public static Dictionary<string, double> SummarizeStrictAccrualTiersForVariance(List<StrictAccrualTier> items)
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

    public static List<ManualHolidayVariance> DeriveManualHolidayVariancesFromTier(List<StrictAccrualTier> items)
    {
        var outValue = new List<ManualHolidayVariance>();
        foreach (var item in items)
        {
            var derived = ManualHolidayVariance.NewManualHolidayVariance(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreManualHolidayVariance(ManualHolidayVariance item)
    {
        return LegacyAccrualClaimPolicy.LegacyAccrualClaimBucket(item.WeightedTotal());
    }
}
