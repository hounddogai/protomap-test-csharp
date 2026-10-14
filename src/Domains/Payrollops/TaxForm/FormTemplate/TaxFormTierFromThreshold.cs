namespace Acme.Workforce.Payrollops.TaxForm.FormTemplate;

using Acme.Workforce.Payrollops.Payslip.Mapping;

/// <summary>Functions of tax form tier from threshold.</summary>
public static class TaxFormTierFromThreshold
{
    public static Dictionary<string, double> SummarizeDefaultPayslipThresholdsForTier(List<DefaultPayslipThreshold> items)
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

    public static List<TaxFormTier> DeriveTaxFormTiersFromThreshold(List<DefaultPayslipThreshold> items)
    {
        var outValue = new List<TaxFormTier>();
        foreach (var item in items)
        {
            var derived = TaxFormTier.NewTaxFormTier(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileTaxFormTier(TaxFormTier item)
    {
        return PayslipMappingPolicy.PayslipMappingBucket(item.NetTotal());
    }
}
