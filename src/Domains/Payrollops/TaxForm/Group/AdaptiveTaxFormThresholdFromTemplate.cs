namespace Acme.Workforce.Payrollops.TaxForm.Group;

using Acme.Workforce.Payrollops.RetroPay.PayBudget;

/// <summary>Functions of adaptive tax form threshold from template.</summary>
public static class AdaptiveTaxFormThresholdFromTemplate
{
    public static Dictionary<string, double> SummarizeRetroPayTemplatesForThreshold(List<RetroPayTemplate> items)
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

    public static List<AdaptiveTaxFormThreshold> DeriveAdaptiveTaxFormThresholdsFromTemplate(List<RetroPayTemplate> items)
    {
        var outValue = new List<AdaptiveTaxFormThreshold>();
        foreach (var item in items)
        {
            var derived = AdaptiveTaxFormThreshold.NewAdaptiveTaxFormThreshold(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchAdaptiveTaxFormThreshold(AdaptiveTaxFormThreshold item)
    {
        return RetroPayTargetPolicy.NormalizeRetroPayTargetCode(item.Code);
    }
}
