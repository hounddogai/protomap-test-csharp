namespace Acme.Workforce.Assets.Laptop.Quota;

using Acme.Workforce.Assets.AssetReturn.Forecast;

/// <summary>Functions of laptop quota from record.</summary>
public static class LaptopQuotaFromRecord
{
    public static Dictionary<string, double> SummarizeGlobalAssetReturnRecordsForQuota(List<GlobalAssetReturnRecord> items)
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

    public static List<LaptopQuota> DeriveLaptopQuotasFromRecord(List<GlobalAssetReturnRecord> items)
    {
        var outValue = new List<LaptopQuota>();
        foreach (var item in items)
        {
            var derived = LaptopQuota.NewLaptopQuota(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveLaptopQuota(LaptopQuota item)
    {
        return RollingAssetReturnForecastPolicy.NormalizeRollingAssetReturnForecastCode(item.Code);
    }
}
