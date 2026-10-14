namespace Acme.Workforce.Assets.Refresh.Window;

using Acme.Workforce.Assets.LicenseSeat.Assignment;

/// <summary>Functions of batched refresh window from rule.</summary>
public static class BatchedRefreshWindowFromRule
{
    public static Dictionary<string, double> SummarizeTieredLicenseSeatRulesForWindow(List<TieredLicenseSeatRule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<BatchedRefreshWindow> DeriveBatchedRefreshWindowsFromRule(List<TieredLicenseSeatRule> items)
    {
        var outValue = new List<BatchedRefreshWindow>();
        foreach (var item in items)
        {
            var derived = BatchedRefreshWindow.NewBatchedRefreshWindow(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceBatchedRefreshWindow(BatchedRefreshWindow item)
    {
        return TieredLicenseSeatRulePolicy.TieredLicenseSeatRuleBucket(item.NetExposure());
    }
}
