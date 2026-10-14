namespace Acme.Workforce.Talent.Tests.SkillProfile.Forecast;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SkillProfile.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class SkillProfileSegmentReportChecks
{
    public static void SkillProfileSegmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707673851);
        var items = new List<SkillProfileSegment>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(SkillProfileSegment.NewSkillProfileSegment($"skill-profile-segment-{index}", "sample"));
        }
        var report = SkillProfileSegmentReport.BuildSkillProfileSegmentReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        SkillProfileSegmentReportCountsEachRecordOnce();
    }
}
