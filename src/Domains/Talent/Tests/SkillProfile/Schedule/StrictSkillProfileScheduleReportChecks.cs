namespace Acme.Workforce.Talent.Tests.SkillProfile.Schedule;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SkillProfile.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class StrictSkillProfileScheduleReportChecks
{
    public static void StrictSkillProfileScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703364186);
        var items = new List<StrictSkillProfileSchedule>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(StrictSkillProfileSchedule.NewStrictSkillProfileSchedule($"strict-skill-profile-schedule-{index}", "sample"));
        }
        var report = StrictSkillProfileScheduleReport.BuildStrictSkillProfileScheduleReport("test", items, clock);
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
        StrictSkillProfileScheduleReportCountsEachRecordOnce();
    }
}
