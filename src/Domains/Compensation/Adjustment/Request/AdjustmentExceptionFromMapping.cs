namespace Acme.Workforce.Compensation.Adjustment.Request;

using Acme.Workforce.Compensation.PayBand.Estimate;

/// <summary>Functions of adjustment exception from mapping.</summary>
public static class AdjustmentExceptionFromMapping
{
    public static Dictionary<string, double> SummarizeFallbackPayBandMappingsForException(List<FallbackPayBandMapping> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<AdjustmentException> DeriveAdjustmentExceptionsFromMapping(List<FallbackPayBandMapping> items)
    {
        var outValue = new List<AdjustmentException>();
        foreach (var item in items)
        {
            var derived = AdjustmentException.NewAdjustmentException(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SummarizeAdjustmentException(AdjustmentException item)
    {
        return SeasonalPayBandQuotaPolicy.SeasonalPayBandQuotaWeight(4, true);
    }
}
