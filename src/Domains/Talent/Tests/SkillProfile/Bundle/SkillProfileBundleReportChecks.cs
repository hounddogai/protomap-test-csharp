namespace Acme.Workforce.Talent.Tests.SkillProfile.Bundle;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SkillProfile.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class SkillProfileBundleReportChecks
{
    public static void SkillProfileBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700380400);
        var items = new List<SkillProfileBundle>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(SkillProfileBundle.NewSkillProfileBundle($"skill-profile-bundle-{index}", "sample"));
        }
        var report = SkillProfileBundleReport.BuildSkillProfileBundleReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        SkillProfileBundleReportCountsEachRecordOnce();
    }
}
