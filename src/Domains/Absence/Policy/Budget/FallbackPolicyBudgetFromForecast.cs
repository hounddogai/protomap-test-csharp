namespace Acme.Workforce.Absence.Policy.Budget;

using Acme.Workforce.Absence.Policy.Estimate;

/// <summary>Functions of fallback policy budget from forecast.</summary>
public static class FallbackPolicyBudgetFromForecast
{
    public static Dictionary<string, double> SummarizePolicyForecastsForBudget(List<PolicyForecast> items)
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

    public static List<FallbackPolicyBudget> DeriveFallbackPolicyBudgetsFromForecast(List<PolicyForecast> items)
    {
        var outValue = new List<FallbackPolicyBudget>();
        foreach (var item in items)
        {
            var derived = FallbackPolicyBudget.NewFallbackPolicyBudget(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleFallbackPolicyBudget(FallbackPolicyBudget item)
    {
        return PolicyEstimatePolicy.PolicyEstimateBucket(item.NetExposure());
    }
}
