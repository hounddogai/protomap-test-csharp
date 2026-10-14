namespace Acme.Workforce.Payrollops.TaxForm.Tier;

using Acme.Workforce.Payrollops.Garnishment.RecordItem;

/// <summary>Functions of tiered tax form bundle from record.</summary>
public static class TieredTaxFormBundleFromRecord
{
    public static Dictionary<string, double> SummarizeCappedGarnishmentRecordsForBundle(List<CappedGarnishmentRecord> items)
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

    public static List<TieredTaxFormBundle> DeriveTieredTaxFormBundlesFromRecord(List<CappedGarnishmentRecord> items)
    {
        var outValue = new List<TieredTaxFormBundle>();
        foreach (var item in items)
        {
            var derived = TieredTaxFormBundle.NewTieredTaxFormBundle(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyTieredTaxFormBundle(TieredTaxFormBundle item)
    {
        return GarnishmentThresholdPolicy.NormalizeGarnishmentThresholdCode(item.Code);
    }
}
