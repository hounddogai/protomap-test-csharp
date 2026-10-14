namespace Acme.Workforce.Engagement.PulseSurvey.Preference;

using Acme.Workforce.Engagement.ActionPlan.Batch;

/// <summary>Functions of default pulse survey override from metric.</summary>
public static class DefaultPulseSurveyOverrideFromMetric
{
    public static Dictionary<string, double> SummarizeActionPlanMetricsForOverride(List<ActionPlanMetric> items)
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

    public static List<DefaultPulseSurveyOverride> DeriveDefaultPulseSurveyOverridesFromMetric(List<ActionPlanMetric> items)
    {
        var outValue = new List<DefaultPulseSurveyOverride>();
        foreach (var item in items)
        {
            var derived = DefaultPulseSurveyOverride.NewDefaultPulseSurveyOverride(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MergeDefaultPulseSurveyOverride(DefaultPulseSurveyOverride item)
    {
        return ActionPlanMetricPolicy.ActionPlanMetricWeight(5, true);
    }
}
