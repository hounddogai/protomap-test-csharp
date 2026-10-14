namespace Acme.Workforce.Compensation.Tests.EquityGrant.Exception;

using Acme.Workforce.Compensation.EquityGrant.Exception;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class EquityGrantExceptionReportChecks
{
    public static void EquityGrantExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704746684);
        var items = new List<EquityGrantException>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(EquityGrantException.NewEquityGrantException($"equity-grant-exception-{index}", "sample"));
        }
        var report = EquityGrantExceptionReport.BuildEquityGrantExceptionReport("test", items, clock);
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
        EquityGrantExceptionReportCountsEachRecordOnce();
    }
}
