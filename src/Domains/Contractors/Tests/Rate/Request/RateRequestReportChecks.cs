namespace Acme.Workforce.Contractors.Tests.Rate.Request;

using Acme.Workforce.Contractors.Rate.Request;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RateRequestReportChecks
{
    public static void RateRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705288534);
        var items = new List<RateRequest>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(RateRequest.NewRateRequest($"rate-request-{index}", "sample"));
        }
        var report = RateRequestReport.BuildRateRequestReport("test", items, clock);
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
        RateRequestReportCountsEachRecordOnce();
    }
}
