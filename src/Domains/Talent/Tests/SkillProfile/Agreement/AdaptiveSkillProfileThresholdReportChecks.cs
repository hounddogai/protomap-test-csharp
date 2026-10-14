namespace Acme.Workforce.Talent.Tests.SkillProfile.Agreement;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SkillProfile.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class AdaptiveSkillProfileThresholdReportChecks
{
    public static void AdaptiveSkillProfileThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700461256);
        var items = new List<AdaptiveSkillProfileThreshold>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AdaptiveSkillProfileThreshold.NewAdaptiveSkillProfileThreshold($"adaptive-skill-profile-threshold-{index}", "sample"));
        }
        var report = AdaptiveSkillProfileThresholdReport.BuildAdaptiveSkillProfileThresholdReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        AdaptiveSkillProfileThresholdReportCountsEachRecordOnce();
    }
}
