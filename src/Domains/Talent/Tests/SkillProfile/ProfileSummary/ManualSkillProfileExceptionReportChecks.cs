namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileSummary;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SkillProfile.ProfileSummary;

/// <summary>Checks of profile summary code, run by the project's runner.</summary>
public static class ManualSkillProfileExceptionReportChecks
{
    public static void ManualSkillProfileExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708037541);
        var items = new List<ManualSkillProfileException>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ManualSkillProfileException.NewManualSkillProfileException($"manual-skill-profile-exception-{index}", "sample"));
        }
        var report = ManualSkillProfileExceptionReport.BuildManualSkillProfileExceptionReport("test", items, clock);
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
        ManualSkillProfileExceptionReportCountsEachRecordOnce();
    }
}
