namespace Acme.Workforce.Assets.Laptop.Audit;

using Acme.Workforce.Assets.Refresh.RefreshRequest;

/// <summary>Functions of rolling laptop exception from override.</summary>
public static class RollingLaptopExceptionFromOverride
{
    public static Dictionary<string, double> SummarizeGlobalRefreshOverridesForException(List<GlobalRefreshOverride> items)
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

    public static List<RollingLaptopException> DeriveRollingLaptopExceptionsFromOverride(List<GlobalRefreshOverride> items)
    {
        var outValue = new List<RollingLaptopException>();
        foreach (var item in items)
        {
            var derived = RollingLaptopException.NewRollingLaptopException(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double BalanceRollingLaptopException(RollingLaptopException item)
    {
        return LenientRefreshRequestPolicy.LenientRefreshRequestWeight(5, true);
    }
}
