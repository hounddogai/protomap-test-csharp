namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileEntry;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SkillProfile.ProfileEntry;

/// <summary>Checks of profile entry code, run by the project's runner.</summary>
public static class StrictSkillProfileEntryReportChecks
{
    public static void StrictSkillProfileEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701145597);
        var items = new List<StrictSkillProfileEntry>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(StrictSkillProfileEntry.NewStrictSkillProfileEntry($"strict-skill-profile-entry-{index}", "sample"));
        }
        var report = StrictSkillProfileEntryReport.BuildStrictSkillProfileEntryReport("test", items, clock);
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
        StrictSkillProfileEntryReportCountsEachRecordOnce();
    }
}
