namespace Acme.Workforce.Payrollops.Payslip.Budget;

using Acme.Workforce.Payrollops.Garnishment.LegacyGarnishmentAgreement;

/// <summary>Functions of fallback payslip template from agreement.</summary>
public static class FallbackPayslipTemplateFromAgreement
{
    public static Dictionary<string, double> SummarizeRegionalGarnishmentAgreementsForTemplate(List<RegionalGarnishmentAgreement> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<FallbackPayslipTemplate> DeriveFallbackPayslipTemplatesFromAgreement(List<RegionalGarnishmentAgreement> items)
    {
        var outValue = new List<FallbackPayslipTemplate>();
        foreach (var item in items)
        {
            var derived = FallbackPayslipTemplate.NewFallbackPayslipTemplate(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateFallbackPayslipTemplate(FallbackPayslipTemplate item)
    {
        return AdaptiveGarnishmentExceptionPolicy.ClassifyAdaptiveGarnishmentException(item.WeightedVolume());
    }
}
