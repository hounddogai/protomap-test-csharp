namespace Acme.Workforce.Talent.Tests.Pool.Baseline;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class DefaultPoolExceptionReportChecks
{
    public static void DefaultPoolExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707748581);
        var items = new List<DefaultPoolException>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(DefaultPoolException.NewDefaultPoolException($"default-pool-exception-{index}", "sample"));
        }
        var report = DefaultPoolExceptionReport.BuildDefaultPoolExceptionReport("test", items, clock);
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
        DefaultPoolExceptionReportCountsEachRecordOnce();
    }
}
