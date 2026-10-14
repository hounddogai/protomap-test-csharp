namespace Acme.Workforce.Talent.Tests.Pool.Exception;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class GlobalPoolExceptionReportChecks
{
    public static void GlobalPoolExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700220757);
        var items = new List<GlobalPoolException>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(GlobalPoolException.NewGlobalPoolException($"global-pool-exception-{index}", "sample"));
        }
        var report = GlobalPoolExceptionReport.BuildGlobalPoolExceptionReport("test", items, clock);
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
        GlobalPoolExceptionReportCountsEachRecordOnce();
    }
}
