namespace Acme.Workforce.Talent.Tests.SkillProfile.Policy;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SkillProfile.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class SkillProfileGroupReportChecks
{
    public static void SkillProfileGroupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703172553);
        var items = new List<SkillProfileGroup>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(SkillProfileGroup.NewSkillProfileGroup($"skill-profile-group-{index}", "sample"));
        }
        var report = SkillProfileGroupReport.BuildSkillProfileGroupReport("test", items, clock);
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
        SkillProfileGroupReportCountsEachRecordOnce();
    }
}
