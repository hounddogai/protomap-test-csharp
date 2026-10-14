namespace Acme.Workforce.Payrollops.Garnishment.Tier;

using Acme.Workforce.Payrollops.Deduction.Proposal;

/// <summary>Functions of garnishment tier from template.</summary>
public static class GarnishmentTierFromTemplate
{
    public static Dictionary<string, double> SummarizeDeductionTemplatesForTier(List<DeductionTemplate> items)
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

    public static List<GarnishmentTier> DeriveGarnishmentTiersFromTemplate(List<DeductionTemplate> items)
    {
        var outValue = new List<GarnishmentTier>();
        foreach (var item in items)
        {
            var derived = GarnishmentTier.NewGarnishmentTier(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyGarnishmentTier(GarnishmentTier item)
    {
        return DeductionTemplatePolicy.ClassifyDeductionTemplate(item.MeasuredValue());
    }
}
