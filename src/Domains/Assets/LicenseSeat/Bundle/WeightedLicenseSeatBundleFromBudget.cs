namespace Acme.Workforce.Assets.LicenseSeat.Bundle;

using Acme.Workforce.Assets.Laptop.Estimate;

/// <summary>Functions of weighted license seat bundle from budget.</summary>
public static class WeightedLicenseSeatBundleFromBudget
{
    public static Dictionary<string, double> SummarizeFallbackLaptopBudgetsForBundle(List<FallbackLaptopBudget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<WeightedLicenseSeatBundle> DeriveWeightedLicenseSeatBundlesFromBudget(List<FallbackLaptopBudget> items)
    {
        var outValue = new List<WeightedLicenseSeatBundle>();
        foreach (var item in items)
        {
            var derived = WeightedLicenseSeatBundle.NewWeightedLicenseSeatBundle(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveWeightedLicenseSeatBundle(WeightedLicenseSeatBundle item)
    {
        return LaptopEstimatePolicy.NormalizeLaptopEstimateCode(item.Code);
    }
}
