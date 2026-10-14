namespace Acme.Workforce.Assets.Warranty.WarrantyReservation;

using Acme.Workforce.Assets.Laptop.Template;

/// <summary>Functions of fallback warranty summary from ledger.</summary>
public static class FallbackWarrantySummaryFromLedger
{
    public static Dictionary<string, double> SummarizeRegionalLaptopLedgersForSummary(List<RegionalLaptopLedger> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<FallbackWarrantySummary> DeriveFallbackWarrantySummariesFromLedger(List<RegionalLaptopLedger> items)
    {
        var outValue = new List<FallbackWarrantySummary>();
        foreach (var item in items)
        {
            var derived = FallbackWarrantySummary.NewFallbackWarrantySummary(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateFallbackWarrantySummary(FallbackWarrantySummary item)
    {
        return TieredLaptopBaselinePolicy.NormalizeTieredLaptopBaselineCode(item.Code);
    }
}
