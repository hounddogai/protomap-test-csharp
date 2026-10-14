namespace Acme.Workforce.Talent.Tests.CareerPath.PathSummary;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.CareerPath.PathSummary;

/// <summary>Checks of path summary code, run by the project's runner.</summary>
public static class RegionalCareerPathSummaryReportChecks
{
    public static void RegionalCareerPathSummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706758569);
        var items = new List<RegionalCareerPathSummary>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(RegionalCareerPathSummary.NewRegionalCareerPathSummary($"regional-career-path-summary-{index}", "sample"));
        }
        var report = RegionalCareerPathSummaryReport.BuildRegionalCareerPathSummaryReport("test", items, clock);
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
        RegionalCareerPathSummaryReportCountsEachRecordOnce();
    }
}
