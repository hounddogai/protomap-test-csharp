namespace Acme.Workforce.Payrollops.TaxForm.Template;

using Acme.Workforce.Payrollops.RetroPay.Summary;

/// <summary>Functions of tax form template from summary.</summary>
public static class TaxFormTemplateFromSummary
{
    public static Dictionary<string, double> SummarizeRetroPaySummariesForTemplate(List<RetroPaySummary> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<TaxFormTemplate> DeriveTaxFormTemplatesFromSummary(List<RetroPaySummary> items)
    {
        var outValue = new List<TaxFormTemplate>();
        foreach (var item in items)
        {
            var derived = TaxFormTemplate.NewTaxFormTemplate(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AssignTaxFormTemplate(TaxFormTemplate item)
    {
        return RollingRetroPaySummaryPolicy.RollingRetroPaySummaryWeight(2, true);
    }
}
