namespace Acme.Workforce.Orgdesign.Tests.Span.Claim;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class RollingSpanExceptionReportChecks
{
    public static void RollingSpanExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701781062);
        var items = new List<RollingSpanException>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RollingSpanException.NewRollingSpanException($"rolling-span-exception-{index}", "sample"));
        }
        var report = RollingSpanExceptionReport.BuildRollingSpanExceptionReport("test", items, clock);
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
        RollingSpanExceptionReportCountsEachRecordOnce();
    }
}
