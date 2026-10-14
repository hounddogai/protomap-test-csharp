namespace Acme.Workforce.Assets.Refresh.Metric;

using Acme.Workforce.Assets.Laptop.Milestone;

/// <summary>Functions of refresh quota from window.</summary>
public static class RefreshQuotaFromWindow
{
    public static Dictionary<string, double> SummarizeLaptopWindowsForQuota(List<LaptopWindow> items)
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

    public static List<RefreshQuota> DeriveRefreshQuotasFromWindow(List<LaptopWindow> items)
    {
        var outValue = new List<RefreshQuota>();
        foreach (var item in items)
        {
            var derived = RefreshQuota.NewRefreshQuota(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceRefreshQuota(RefreshQuota item)
    {
        return LaptopSegmentPolicy.NormalizeLaptopSegmentCode(item.Code);
    }
}
