namespace Acme.Workforce.Talent.Tests.CareerPath.PathOverride;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.CareerPath.PathOverride;

/// <summary>Checks of path override code, run by the project's runner.</summary>
public static class StrictCareerPathOverrideReportChecks
{
    public static void StrictCareerPathOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701791245);
        var items = new List<StrictCareerPathOverride>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(StrictCareerPathOverride.NewStrictCareerPathOverride($"strict-career-path-override-{index}", "sample"));
        }
        var report = StrictCareerPathOverrideReport.BuildStrictCareerPathOverrideReport("test", items, clock);
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
        StrictCareerPathOverrideReportCountsEachRecordOnce();
    }
}
