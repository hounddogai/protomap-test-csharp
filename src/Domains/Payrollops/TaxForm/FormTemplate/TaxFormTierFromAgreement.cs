namespace Acme.Workforce.Payrollops.TaxForm.FormTemplate;

using Acme.Workforce.Payrollops.PayRun.Window;

/// <summary>Functions of tax form tier from agreement.</summary>
public static class TaxFormTierFromAgreement
{
    public static Dictionary<string, double> SummarizeLenientPayRunAgreementsForTier(List<LenientPayRunAgreement> items)
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

    public static List<TaxFormTier> DeriveTaxFormTiersFromAgreement(List<LenientPayRunAgreement> items)
    {
        var outValue = new List<TaxFormTier>();
        foreach (var item in items)
        {
            var derived = TaxFormTier.NewTaxFormTier(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeTaxFormTier(TaxFormTier item)
    {
        return LenientPayRunAgreementPolicy.ClassifyLenientPayRunAgreement(item.NetTotal());
    }
}
