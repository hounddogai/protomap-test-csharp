namespace Acme.Workforce.Payrollops.Garnishment.Preference;

using Acme.Workforce.Payrollops.TaxForm.FormBundle;

/// <summary>Functions of tiered garnishment quota from threshold.</summary>
public static class TieredGarnishmentQuotaFromThreshold
{
    public static Dictionary<string, double> SummarizePendingTaxFormThresholdsForQuota(List<PendingTaxFormThreshold> items)
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

    public static List<TieredGarnishmentQuota> DeriveTieredGarnishmentQuotasFromThreshold(List<PendingTaxFormThreshold> items)
    {
        var outValue = new List<TieredGarnishmentQuota>();
        foreach (var item in items)
        {
            var derived = TieredGarnishmentQuota.NewTieredGarnishmentQuota(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ScoreTieredGarnishmentQuota(TieredGarnishmentQuota item)
    {
        return LenientTaxFormAssignmentPolicy.LenientTaxFormAssignmentWeight(3, true);
    }
}
