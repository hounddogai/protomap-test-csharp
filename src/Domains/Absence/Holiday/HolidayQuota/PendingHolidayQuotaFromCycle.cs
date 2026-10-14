namespace Acme.Workforce.Absence.Holiday.HolidayQuota;

using Acme.Workforce.Absence.Balance.Preference;

/// <summary>Functions of pending holiday quota from cycle.</summary>
public static class PendingHolidayQuotaFromCycle
{
    public static Dictionary<string, double> SummarizeFallbackBalanceCyclesForQuota(List<FallbackBalanceCycle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<PendingHolidayQuota> DerivePendingHolidayQuotasFromCycle(List<FallbackBalanceCycle> items)
    {
        var outValue = new List<PendingHolidayQuota>();
        foreach (var item in items)
        {
            var derived = PendingHolidayQuota.NewPendingHolidayQuota(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizePendingHolidayQuota(PendingHolidayQuota item)
    {
        return FallbackBalanceCyclePolicy.NormalizeFallbackBalanceCycleCode(item.Code);
    }
}
