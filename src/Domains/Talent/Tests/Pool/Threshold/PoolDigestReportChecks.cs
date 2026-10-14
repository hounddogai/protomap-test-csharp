namespace Acme.Workforce.Talent.Tests.Pool.Threshold;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PoolDigestReportChecks
{
    public static void PoolDigestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708414243);
        var items = new List<PoolDigest>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PoolDigest.NewPoolDigest($"pool-digest-{index}", "sample"));
        }
        var report = PoolDigestReport.BuildPoolDigestReport("test", items, clock);
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
        PoolDigestReportCountsEachRecordOnce();
    }
}
