namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantDigest;

using Acme.Workforce.Compensation.EquityGrant.GrantDigest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of grant digest code, run by the project's runner.</summary>
public static class EquityGrantThresholdReportChecks
{
    public static void EquityGrantThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704998038);
        var items = new List<EquityGrantThreshold>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(EquityGrantThreshold.NewEquityGrantThreshold($"equity-grant-threshold-{index}", "sample"));
        }
        var report = EquityGrantThresholdReport.BuildEquityGrantThresholdReport("test", items, clock);
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
        EquityGrantThresholdReportCountsEachRecordOnce();
    }
}
