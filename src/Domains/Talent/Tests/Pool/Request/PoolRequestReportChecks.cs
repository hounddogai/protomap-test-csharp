namespace Acme.Workforce.Talent.Tests.Pool.Request;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class PoolRequestReportChecks
{
    public static void PoolRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703892588);
        var items = new List<PoolRequest>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PoolRequest.NewPoolRequest($"pool-request-{index}", "sample"));
        }
        var report = PoolRequestReport.BuildPoolRequestReport("test", items, clock);
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
        PoolRequestReportCountsEachRecordOnce();
    }
}
