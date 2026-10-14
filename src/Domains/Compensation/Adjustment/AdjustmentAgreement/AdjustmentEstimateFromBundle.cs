namespace Acme.Workforce.Compensation.Adjustment.AdjustmentAgreement;

using Acme.Workforce.Compensation.Adjustment.Template;

/// <summary>Functions of adjustment estimate from bundle.</summary>
public static class AdjustmentEstimateFromBundle
{
    public static Dictionary<string, double> SummarizeAdjustmentBundlesForEstimate(List<AdjustmentBundle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<AdjustmentEstimate> DeriveAdjustmentEstimatesFromBundle(List<AdjustmentBundle> items)
    {
        var outValue = new List<AdjustmentEstimate>();
        foreach (var item in items)
        {
            var derived = AdjustmentEstimate.NewAdjustmentEstimate(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanAdjustmentEstimate(AdjustmentEstimate item)
    {
        return RollingAdjustmentTemplatePolicy.ClassifyRollingAdjustmentTemplate(item.EffectiveValue());
    }
}
