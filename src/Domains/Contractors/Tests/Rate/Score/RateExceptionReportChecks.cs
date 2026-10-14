namespace Acme.Workforce.Contractors.Tests.Rate.Score;

using Acme.Workforce.Contractors.Rate.Score;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class RateExceptionReportChecks
{
    public static void RateExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705968584);
        var items = new List<RateException>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RateException.NewRateException($"rate-exception-{index}", "sample"));
        }
        var report = RateExceptionReport.BuildRateExceptionReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        RateExceptionReportCountsEachRecordOnce();
    }
}
