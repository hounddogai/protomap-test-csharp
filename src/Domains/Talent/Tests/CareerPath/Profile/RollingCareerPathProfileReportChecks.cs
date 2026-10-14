namespace Acme.Workforce.Talent.Tests.CareerPath.Profile;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.CareerPath.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class RollingCareerPathProfileReportChecks
{
    public static void RollingCareerPathProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704906661);
        var items = new List<RollingCareerPathProfile>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RollingCareerPathProfile.NewRollingCareerPathProfile($"rolling-career-path-profile-{index}", "sample"));
        }
        var report = RollingCareerPathProfileReport.BuildRollingCareerPathProfileReport("test", items, clock);
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
        RollingCareerPathProfileReportCountsEachRecordOnce();
    }
}
