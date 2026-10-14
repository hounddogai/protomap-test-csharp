namespace Acme.Workforce.Absence.Accrual.Manifest;

using Acme.Workforce.Absence.Balance.Threshold;

/// <summary>Functions of default accrual manifest from adjustment.</summary>
public static class DefaultAccrualManifestFromAdjustment
{
    public static Dictionary<string, double> SummarizeBalanceAdjustmentsForManifest(List<BalanceAdjustment> items)
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

    public static List<DefaultAccrualManifest> DeriveDefaultAccrualManifestsFromAdjustment(List<BalanceAdjustment> items)
    {
        var outValue = new List<DefaultAccrualManifest>();
        foreach (var item in items)
        {
            var derived = DefaultAccrualManifest.NewDefaultAccrualManifest(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveDefaultAccrualManifest(DefaultAccrualManifest item)
    {
        return BalanceThresholdPolicy.ClassifyBalanceThreshold(item.NetExposure());
    }
}
