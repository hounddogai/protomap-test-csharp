namespace Acme.Workforce.Payrollops.Tests.PayRun.Adjustment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.PayRun.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class RollingPayRunDigestReportChecks
{
    public static void RollingPayRunDigestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703565872);
        var items = new List<RollingPayRunDigest>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RollingPayRunDigest.NewRollingPayRunDigest($"rolling-pay-run-digest-{index}", "sample"));
        }
        var report = RollingPayRunDigestReport.BuildRollingPayRunDigestReport("test", items, clock);
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
        RollingPayRunDigestReportCountsEachRecordOnce();
    }
}
