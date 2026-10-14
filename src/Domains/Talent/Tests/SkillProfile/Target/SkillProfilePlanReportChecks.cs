namespace Acme.Workforce.Talent.Tests.SkillProfile.Target;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SkillProfile.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class SkillProfilePlanReportChecks
{
    public static void SkillProfilePlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707488991);
        var items = new List<SkillProfilePlan>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(SkillProfilePlan.NewSkillProfilePlan($"skill-profile-plan-{index}", "sample"));
        }
        var report = SkillProfilePlanReport.BuildSkillProfilePlanReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        SkillProfilePlanReportCountsEachRecordOnce();
    }
}
