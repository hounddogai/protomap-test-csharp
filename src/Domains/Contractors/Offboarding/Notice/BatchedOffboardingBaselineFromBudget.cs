namespace Acme.Workforce.Contractors.Offboarding.Notice;

using Acme.Workforce.Contractors.Rate.Budget;

/// <summary>Functions of batched offboarding baseline from budget.</summary>
public static class BatchedOffboardingBaselineFromBudget
{
    public static Dictionary<string, double> SummarizeRateBudgetsForBaseline(List<RateBudget> items)
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

    public static List<BatchedOffboardingBaseline> DeriveBatchedOffboardingBaselinesFromBudget(List<RateBudget> items)
    {
        var outValue = new List<BatchedOffboardingBaseline>();
        foreach (var item in items)
        {
            var derived = BatchedOffboardingBaseline.NewBatchedOffboardingBaseline(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeBatchedOffboardingBaseline(BatchedOffboardingBaseline item)
    {
        return LenientRateForecastPolicy.ClassifyLenientRateForecast(item.AdjustedExposure());
    }
}
