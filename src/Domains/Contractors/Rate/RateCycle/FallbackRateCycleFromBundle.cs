namespace Acme.Workforce.Contractors.Rate.RateCycle;

using Acme.Workforce.Contractors.Offboarding.OffboardingBundle;

/// <summary>Functions of fallback rate cycle from bundle.</summary>
public static class FallbackRateCycleFromBundle
{
    public static Dictionary<string, double> SummarizeStrictOffboardingBundlesForCycle(List<StrictOffboardingBundle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<FallbackRateCycle> DeriveFallbackRateCyclesFromBundle(List<StrictOffboardingBundle> items)
    {
        var outValue = new List<FallbackRateCycle>();
        foreach (var item in items)
        {
            var derived = FallbackRateCycle.NewFallbackRateCycle(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateFallbackRateCycle(FallbackRateCycle item)
    {
        return LegacyOffboardingRequestPolicy.NormalizeLegacyOffboardingRequestCode(item.Code);
    }
}
