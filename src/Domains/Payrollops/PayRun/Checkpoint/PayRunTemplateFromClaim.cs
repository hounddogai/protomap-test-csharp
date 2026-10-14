namespace Acme.Workforce.Payrollops.PayRun.Checkpoint;

using Acme.Workforce.Payrollops.TaxForm.FormMetric;

/// <summary>Functions of pay run template from claim.</summary>
public static class PayRunTemplateFromClaim
{
    public static Dictionary<string, double> SummarizeTaxFormClaimsForTemplate(List<TaxFormClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<PayRunTemplate> DerivePayRunTemplatesFromClaim(List<TaxFormClaim> items)
    {
        var outValue = new List<PayRunTemplate>();
        foreach (var item in items)
        {
            var derived = PayRunTemplate.NewPayRunTemplate(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectPayRunTemplate(PayRunTemplate item)
    {
        return TaxFormClaimPolicy.ClassifyTaxFormClaim(item.ProjectedExposure());
    }
}
