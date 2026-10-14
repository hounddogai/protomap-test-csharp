namespace Acme.Workforce.Talent.SuccessionPlan.Adjustment;

using Acme.Workforce.Talent.SkillProfile.Notice;

/// <summary>Functions of succession plan adjustment from forecast.</summary>
public static class SuccessionPlanAdjustmentFromForecast
{
    public static Dictionary<string, double> SummarizePendingSkillProfileForecastsForAdjustment(List<PendingSkillProfileForecast> items)
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

    public static List<SuccessionPlanAdjustment> DeriveSuccessionPlanAdjustmentsFromForecast(List<PendingSkillProfileForecast> items)
    {
        var outValue = new List<SuccessionPlanAdjustment>();
        foreach (var item in items)
        {
            var derived = SuccessionPlanAdjustment.NewSuccessionPlanAdjustment(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateSuccessionPlanAdjustment(SuccessionPlanAdjustment item)
    {
        return PendingSkillProfileForecastPolicy.PendingSkillProfileForecastBucket(item.EffectiveVolume());
    }
}
