namespace Acme.Workforce.Talent.Tests.CareerPath.GlobalPathRule;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.CareerPath.GlobalPathRule;

/// <summary>Checks of global path rule code, run by the project's runner.</summary>
public static class AdaptiveCareerPathExceptionReportChecks
{
    public static void AdaptiveCareerPathExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706449688);
        var items = new List<AdaptiveCareerPathException>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AdaptiveCareerPathException.NewAdaptiveCareerPathException($"adaptive-career-path-exception-{index}", "sample"));
        }
        var report = AdaptiveCareerPathExceptionReport.BuildAdaptiveCareerPathExceptionReport("test", items, clock);
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
        AdaptiveCareerPathExceptionReportCountsEachRecordOnce();
    }
}
