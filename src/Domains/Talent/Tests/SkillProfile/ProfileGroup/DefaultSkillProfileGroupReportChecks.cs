namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileGroup;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SkillProfile.ProfileGroup;

/// <summary>Checks of profile group code, run by the project's runner.</summary>
public static class DefaultSkillProfileGroupReportChecks
{
    public static void DefaultSkillProfileGroupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705841280);
        var items = new List<DefaultSkillProfileGroup>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(DefaultSkillProfileGroup.NewDefaultSkillProfileGroup($"default-skill-profile-group-{index}", "sample"));
        }
        var report = DefaultSkillProfileGroupReport.BuildDefaultSkillProfileGroupReport("test", items, clock);
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
        DefaultSkillProfileGroupReportCountsEachRecordOnce();
    }
}
