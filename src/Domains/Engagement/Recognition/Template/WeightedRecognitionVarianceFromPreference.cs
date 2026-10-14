namespace Acme.Workforce.Engagement.Recognition.Template;

using Acme.Workforce.Engagement.ActionPlan.PlanPreference;

/// <summary>Functions of weighted recognition variance from preference.</summary>
public static class WeightedRecognitionVarianceFromPreference
{
    public static Dictionary<string, double> SummarizeDefaultActionPlanPreferencesForVariance(List<DefaultActionPlanPreference> items)
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

    public static List<WeightedRecognitionVariance> DeriveWeightedRecognitionVariancesFromPreference(List<DefaultActionPlanPreference> items)
    {
        var outValue = new List<WeightedRecognitionVariance>();
        foreach (var item in items)
        {
            var derived = WeightedRecognitionVariance.NewWeightedRecognitionVariance(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeWeightedRecognitionVariance(WeightedRecognitionVariance item)
    {
        return DefaultActionPlanPreferencePolicy.NormalizeDefaultActionPlanPreferenceCode(item.Code);
    }
}
