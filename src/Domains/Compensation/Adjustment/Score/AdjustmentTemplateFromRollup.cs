namespace Acme.Workforce.Compensation.Adjustment.Score;

using Acme.Workforce.Compensation.PayBand.Rollup;

/// <summary>Functions of adjustment template from rollup.</summary>
public static class AdjustmentTemplateFromRollup
{
    public static Dictionary<string, double> SummarizePayBandRollupsForTemplate(List<PayBandRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<AdjustmentTemplate> DeriveAdjustmentTemplatesFromRollup(List<PayBandRollup> items)
    {
        var outValue = new List<AdjustmentTemplate>();
        foreach (var item in items)
        {
            var derived = AdjustmentTemplate.NewAdjustmentTemplate(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ApplyAdjustmentTemplate(AdjustmentTemplate item)
    {
        return PayBandForecastPolicy.PayBandForecastWeight(5, true);
    }
}
