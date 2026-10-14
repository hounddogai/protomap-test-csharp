namespace Acme.Workforce.Contractors.Tests.Rate.RateNotice;

using Acme.Workforce.Contractors.Rate.RateNotice;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rate notice code, run by the project's runner.</summary>
public static class GlobalRateExceptionReportChecks
{
    public static void GlobalRateExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704339642);
        var items = new List<GlobalRateException>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(GlobalRateException.NewGlobalRateException($"global-rate-exception-{index}", "sample"));
        }
        var report = GlobalRateExceptionReport.BuildGlobalRateExceptionReport("test", items, clock);
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
        GlobalRateExceptionReportCountsEachRecordOnce();
    }
}
