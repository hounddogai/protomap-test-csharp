namespace Acme.Workforce.Talent.SuccessionPlan.Audit;

using Acme.Workforce.Talent.SkillProfile.Schedule;

/// <summary>Functions of rolling succession plan override from schedule.</summary>
public static class RollingSuccessionPlanOverrideFromSchedule
{
    public static Dictionary<string, double> SummarizeDefaultSkillProfileSchedulesForOverride(List<DefaultSkillProfileSchedule> items)
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

    public static List<RollingSuccessionPlanOverride> DeriveRollingSuccessionPlanOverridesFromSchedule(List<DefaultSkillProfileSchedule> items)
    {
        var outValue = new List<RollingSuccessionPlanOverride>();
        foreach (var item in items)
        {
            var derived = RollingSuccessionPlanOverride.NewRollingSuccessionPlanOverride(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceRollingSuccessionPlanOverride(RollingSuccessionPlanOverride item)
    {
        return SkillProfileBaselinePolicy.SkillProfileBaselineBucket(item.EffectiveValue());
    }
}
