namespace Acme.Workforce.Payrollops.TaxForm.Adjustment;

using Acme.Workforce.Payrollops.TaxForm.Template;

/// <summary>Functions of tax form draft from template.</summary>
public static class TaxFormDraftFromTemplate
{
    public static Dictionary<string, double> SummarizeTaxFormTemplatesForDraft(List<TaxFormTemplate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<TaxFormDraft> DeriveTaxFormDraftsFromTemplate(List<TaxFormTemplate> items)
    {
        var outValue = new List<TaxFormDraft>();
        foreach (var item in items)
        {
            var derived = TaxFormDraft.NewTaxFormDraft(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AllocateTaxFormDraft(TaxFormDraft item)
    {
        return TaxFormTemplatePolicy.TaxFormTemplateWeight(1, true);
    }
}
