namespace Acme.Workforce.Assets.AssetReturn.Budget;

using Acme.Workforce.Assets.LicenseSeat.Bundle;

/// <summary>Functions of rolling asset return plan from window.</summary>
public static class RollingAssetReturnPlanFromWindow
{
    public static Dictionary<string, double> SummarizeStrictLicenseSeatWindowsForPlan(List<StrictLicenseSeatWindow> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<RollingAssetReturnPlan> DeriveRollingAssetReturnPlansFromWindow(List<StrictLicenseSeatWindow> items)
    {
        var outValue = new List<RollingAssetReturnPlan>();
        foreach (var item in items)
        {
            var derived = RollingAssetReturnPlan.NewRollingAssetReturnPlan(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double DeriveRollingAssetReturnPlan(RollingAssetReturnPlan item)
    {
        return SeasonalLicenseSeatPreferencePolicy.SeasonalLicenseSeatPreferenceWeight(3, true);
    }
}
