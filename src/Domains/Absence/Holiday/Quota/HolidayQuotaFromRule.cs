namespace Acme.Workforce.Absence.Holiday.Quota;

using Acme.Workforce.Absence.LeaveRequest.Budget;

/// <summary>Functions of holiday quota from rule.</summary>
public static class HolidayQuotaFromRule
{
    public static Dictionary<string, double> SummarizeLeaveRequestRulesForQuota(List<LeaveRequestRule> items)
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

    public static List<HolidayQuota> DeriveHolidayQuotasFromRule(List<LeaveRequestRule> items)
    {
        var outValue = new List<HolidayQuota>();
        foreach (var item in items)
        {
            var derived = HolidayQuota.NewHolidayQuota(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceHolidayQuota(HolidayQuota item)
    {
        return LeaveRequestBatchPolicy.ClassifyLeaveRequestBatch(item.WeightedExposure());
    }
}
