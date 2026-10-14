namespace Acme.Workforce.Absence.Holiday.HolidayQuota;

using Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

/// <summary>Functions of pending holiday quota from ledger.</summary>
public static class PendingHolidayQuotaFromLedger
{
    public static Dictionary<string, double> SummarizeSeasonalAccrualLedgersForQuota(List<SeasonalAccrualLedger> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<PendingHolidayQuota> DerivePendingHolidayQuotasFromLedger(List<SeasonalAccrualLedger> items)
    {
        var outValue = new List<PendingHolidayQuota>();
        foreach (var item in items)
        {
            var derived = PendingHolidayQuota.NewPendingHolidayQuota(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ApplyPendingHolidayQuota(PendingHolidayQuota item)
    {
        return LenientAccrualOverridePolicy.LenientAccrualOverrideWeight(4, true);
    }
}
