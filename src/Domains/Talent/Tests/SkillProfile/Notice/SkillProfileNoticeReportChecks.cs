namespace Acme.Workforce.Talent.Tests.SkillProfile.Notice;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SkillProfile.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class SkillProfileNoticeReportChecks
{
    public static void SkillProfileNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707401493);
        var items = new List<SkillProfileNotice>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(SkillProfileNotice.NewSkillProfileNotice($"skill-profile-notice-{index}", "sample"));
        }
        var report = SkillProfileNoticeReport.BuildSkillProfileNoticeReport("test", items, clock);
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
        SkillProfileNoticeReportCountsEachRecordOnce();
    }
}
