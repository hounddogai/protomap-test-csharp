namespace Acme.Workforce.Payrollops.Garnishment.LegacyGarnishmentAgreement;

using Acme.Workforce.Payrollops.TaxForm.FormEstimate;

/// <summary>Functions of adaptive garnishment exception from window.</summary>
public static class AdaptiveGarnishmentExceptionFromWindow
{
    public static Dictionary<string, double> SummarizeTaxFormWindowsForException(List<TaxFormWindow> items)
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

    public static List<AdaptiveGarnishmentException> DeriveAdaptiveGarnishmentExceptionsFromWindow(List<TaxFormWindow> items)
    {
        var outValue = new List<AdaptiveGarnishmentException>();
        foreach (var item in items)
        {
            var derived = AdaptiveGarnishmentException.NewAdaptiveGarnishmentException(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreAdaptiveGarnishmentException(AdaptiveGarnishmentException item)
    {
        return AdaptiveTaxFormLedgerPolicy.ClassifyAdaptiveTaxFormLedger(item.AdjustedTotal());
    }
}
