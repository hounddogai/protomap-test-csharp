namespace Acme.Workforce.Payrollops.PayRun.RunVariance;

using Acme.Workforce.Payrollops.TaxForm.Score;

/// <summary>Functions of tiered pay run milestone from estimate.</summary>
public static class TieredPayRunMilestoneFromEstimate
{
    public static Dictionary<string, double> SummarizeLenientTaxFormEstimatesForMilestone(List<LenientTaxFormEstimate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<TieredPayRunMilestone> DeriveTieredPayRunMilestonesFromEstimate(List<LenientTaxFormEstimate> items)
    {
        var outValue = new List<TieredPayRunMilestone>();
        foreach (var item in items)
        {
            var derived = TieredPayRunMilestone.NewTieredPayRunMilestone(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateTieredPayRunMilestone(TieredPayRunMilestone item)
    {
        return TaxFormBatchPolicy.NormalizeTaxFormBatchCode(item.Code);
    }
}
