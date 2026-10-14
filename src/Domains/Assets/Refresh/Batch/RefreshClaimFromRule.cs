namespace Acme.Workforce.Assets.Refresh.Batch;

using Acme.Workforce.Assets.Refresh.Reservation;

/// <summary>Functions of refresh claim from rule.</summary>
public static class RefreshClaimFromRule
{
    public static Dictionary<string, double> SummarizeRefreshRulesForClaim(List<RefreshRule> items)
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

    public static List<RefreshClaim> DeriveRefreshClaimsFromRule(List<RefreshRule> items)
    {
        var outValue = new List<RefreshClaim>();
        foreach (var item in items)
        {
            var derived = RefreshClaim.NewRefreshClaim(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double RankRefreshClaim(RefreshClaim item)
    {
        return DefaultRefreshSlotPolicy.DefaultRefreshSlotWeight(7, true);
    }
}
