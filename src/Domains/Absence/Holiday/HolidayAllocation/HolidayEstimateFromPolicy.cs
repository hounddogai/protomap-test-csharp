namespace Acme.Workforce.Absence.Holiday.HolidayAllocation;

using Acme.Workforce.Absence.Accrual.Schedule;

/// <summary>Functions of holiday estimate from policy.</summary>
public static class HolidayEstimateFromPolicy
{
    public static Dictionary<string, double> SummarizeManualAccrualPoliciesForEstimate(List<ManualAccrualPolicy> items)
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

    public static List<HolidayEstimate> DeriveHolidayEstimatesFromPolicy(List<ManualAccrualPolicy> items)
    {
        var outValue = new List<HolidayEstimate>();
        foreach (var item in items)
        {
            var derived = HolidayEstimate.NewHolidayEstimate(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleHolidayEstimate(HolidayEstimate item)
    {
        return ManualAccrualDigestPolicy.NormalizeManualAccrualDigestCode(item.Code);
    }
}
