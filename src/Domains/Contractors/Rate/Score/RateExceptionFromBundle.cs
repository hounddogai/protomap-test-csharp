namespace Acme.Workforce.Contractors.Rate.Score;

using Acme.Workforce.Contractors.Rate.Window;

/// <summary>Functions of rate exception from bundle.</summary>
public static class RateExceptionFromBundle
{
    public static Dictionary<string, double> SummarizeFallbackRateBundlesForException(List<FallbackRateBundle> items)
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

    public static List<RateException> DeriveRateExceptionsFromBundle(List<FallbackRateBundle> items)
    {
        var outValue = new List<RateException>();
        foreach (var item in items)
        {
            var derived = RateException.NewRateException(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ComputeRateException(RateException item)
    {
        return FallbackRateBundlePolicy.FallbackRateBundleWeight(5, true);
    }
}
