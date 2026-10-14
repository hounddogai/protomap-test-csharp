namespace Acme.Workforce.Payrollops.TaxForm.Estimate;

using Acme.Workforce.Payrollops.RetroPay.Tier;

/// <summary>Functions of lenient tax form notice from preference.</summary>
public static class LenientTaxFormNoticeFromPreference
{
    public static Dictionary<string, double> SummarizeCappedRetroPayPreferencesForNotice(List<CappedRetroPayPreference> items)
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

    public static List<LenientTaxFormNotice> DeriveLenientTaxFormNoticesFromPreference(List<CappedRetroPayPreference> items)
    {
        var outValue = new List<LenientTaxFormNotice>();
        foreach (var item in items)
        {
            var derived = LenientTaxFormNotice.NewLenientTaxFormNotice(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeLenientTaxFormNotice(LenientTaxFormNotice item)
    {
        return WeightedRetroPayNoticePolicy.WeightedRetroPayNoticeBucket(item.AdjustedLoad());
    }
}
