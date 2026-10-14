namespace Acme.Workforce.Compensation.EquityGrant.Tier;

using Acme.Workforce.Compensation.EquityGrant.Digest;

/// <summary>Functions of equity grant window from digest.</summary>
public static class EquityGrantWindowFromDigest
{
    public static Dictionary<string, double> SummarizeEquityGrantDigestsForWindow(List<EquityGrantDigest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<EquityGrantWindow> DeriveEquityGrantWindowsFromDigest(List<EquityGrantDigest> items)
    {
        var outValue = new List<EquityGrantWindow>();
        foreach (var item in items)
        {
            var derived = EquityGrantWindow.NewEquityGrantWindow(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleEquityGrantWindow(EquityGrantWindow item)
    {
        return EquityGrantLedgerPolicy.EquityGrantLedgerWeight(6, true);
    }
}
