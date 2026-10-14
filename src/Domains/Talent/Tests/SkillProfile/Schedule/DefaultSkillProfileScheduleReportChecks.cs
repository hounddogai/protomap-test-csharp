namespace Acme.Workforce.Talent.Tests.SkillProfile.Schedule;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SkillProfile.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class DefaultSkillProfileScheduleReportChecks
{
    public static void DefaultSkillProfileScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702090179);
        var items = new List<DefaultSkillProfileSchedule>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(DefaultSkillProfileSchedule.NewDefaultSkillProfileSchedule($"default-skill-profile-schedule-{index}", "sample"));
        }
        var report = DefaultSkillProfileScheduleReport.BuildDefaultSkillProfileScheduleReport("test", items, clock);
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
        DefaultSkillProfileScheduleReportCountsEachRecordOnce();
    }
}
