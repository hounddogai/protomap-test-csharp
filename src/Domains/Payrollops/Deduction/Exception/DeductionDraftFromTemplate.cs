namespace Acme.Workforce.Payrollops.Deduction.Exception;

using Acme.Workforce.Payrollops.Deduction.Proposal;

/// <summary>Functions of deduction draft from template.</summary>
public static class DeductionDraftFromTemplate
{
    public static Dictionary<string, double> SummarizeDeductionTemplatesForDraft(List<DeductionTemplate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<DeductionDraft> DeriveDeductionDraftsFromTemplate(List<DeductionTemplate> items)
    {
        var outValue = new List<DeductionDraft>();
        foreach (var item in items)
        {
            var derived = DeductionDraft.NewDeductionDraft(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateDeductionDraft(DeductionDraft item)
    {
        return DeductionTemplatePolicy.ClassifyDeductionTemplate(item.NetValue());
    }
}
