namespace Acme.Workforce.Payrollops.TaxForm.Bundle;

using Acme.Workforce.Payrollops.Deduction.Baseline;

/// <summary>Functions of tax form bundle from notice.</summary>
public static class TaxFormBundleFromNotice
{
    public static Dictionary<string, double> SummarizeLenientDeductionNoticesForBundle(List<LenientDeductionNotice> items)
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

    public static List<TaxFormBundle> DeriveTaxFormBundlesFromNotice(List<LenientDeductionNotice> items)
    {
        var outValue = new List<TaxFormBundle>();
        foreach (var item in items)
        {
            var derived = TaxFormBundle.NewTaxFormBundle(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectTaxFormBundle(TaxFormBundle item)
    {
        return DeductionBaselinePolicy.ClassifyDeductionBaseline(item.WeightedTotal());
    }
}
