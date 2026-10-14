namespace Acme.Workforce.Talent.Tests.SkillProfile.Target;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SkillProfile.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class SkillProfileRequestReportChecks
{
    public static void SkillProfileRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705587080);
        var items = new List<SkillProfileRequest>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(SkillProfileRequest.NewSkillProfileRequest($"skill-profile-request-{index}", "sample"));
        }
        var report = SkillProfileRequestReport.BuildSkillProfileRequestReport("test", items, clock);
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
        SkillProfileRequestReportCountsEachRecordOnce();
    }
}
