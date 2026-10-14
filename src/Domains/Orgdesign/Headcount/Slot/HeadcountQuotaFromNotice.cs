namespace Acme.Workforce.Orgdesign.Headcount.Slot;

using Acme.Workforce.Orgdesign.Reorg.Rule;

/// <summary>Functions of headcount quota from notice.</summary>
public static class HeadcountQuotaFromNotice
{
    public static Dictionary<string, double> SummarizeStrictReorgNoticesForQuota(List<StrictReorgNotice> items)
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

    public static List<HeadcountQuota> DeriveHeadcountQuotasFromNotice(List<StrictReorgNotice> items)
    {
        var outValue = new List<HeadcountQuota>();
        foreach (var item in items)
        {
            var derived = HeadcountQuota.NewHeadcountQuota(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyHeadcountQuota(HeadcountQuota item)
    {
        return StrictReorgNoticePolicy.NormalizeStrictReorgNoticeCode(item.Code);
    }
}
