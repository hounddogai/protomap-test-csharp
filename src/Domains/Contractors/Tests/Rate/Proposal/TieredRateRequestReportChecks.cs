namespace Acme.Workforce.Contractors.Tests.Rate.Proposal;

using Acme.Workforce.Contractors.Rate.Proposal;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class TieredRateRequestReportChecks
{
    public static void TieredRateRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706078654);
        var items = new List<TieredRateRequest>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(TieredRateRequest.NewTieredRateRequest($"tiered-rate-request-{index}", "sample"));
        }
        var report = TieredRateRequestReport.BuildTieredRateRequestReport("test", items, clock);
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
        TieredRateRequestReportCountsEachRecordOnce();
    }
}
