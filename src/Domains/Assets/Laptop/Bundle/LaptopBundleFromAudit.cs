namespace Acme.Workforce.Assets.Laptop.Bundle;

using Acme.Workforce.Assets.LicenseSeat.Audit;

/// <summary>Functions of laptop bundle from audit.</summary>
public static class LaptopBundleFromAudit
{
    public static Dictionary<string, double> SummarizeLicenseSeatAuditsForBundle(List<LicenseSeatAudit> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<LaptopBundle> DeriveLaptopBundlesFromAudit(List<LicenseSeatAudit> items)
    {
        var outValue = new List<LaptopBundle>();
        foreach (var item in items)
        {
            var derived = LaptopBundle.NewLaptopBundle(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveLaptopBundle(LaptopBundle item)
    {
        return LicenseSeatProfilePolicy.NormalizeLicenseSeatProfileCode(item.Code);
    }
}
