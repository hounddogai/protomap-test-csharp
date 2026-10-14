namespace Acme.Workforce.Payrollops.TaxForm.Template;

using Acme.Workforce.Payrollops.Garnishment.Profile;

/// <summary>Functions of tax form template from digest.</summary>
public static class TaxFormTemplateFromDigest
{
    public static Dictionary<string, double> SummarizeGarnishmentDigestsForTemplate(List<GarnishmentDigest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<TaxFormTemplate> DeriveTaxFormTemplatesFromDigest(List<GarnishmentDigest> items)
    {
        var outValue = new List<TaxFormTemplate>();
        foreach (var item in items)
        {
            var derived = TaxFormTemplate.NewTaxFormTemplate(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyTaxFormTemplateExtra(TaxFormTemplate item)
    {
        return GarnishmentProfilePolicy.GarnishmentProfileBucket(item.AdjustedValue());
    }
}
