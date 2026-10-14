namespace Acme.Workforce.Talent.Tests.Pool.Quota;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class PoolQuotaReportChecks
{
    public static void PoolQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705013459);
        var items = new List<PoolQuota>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PoolQuota.NewPoolQuota($"pool-quota-{index}", "sample"));
        }
        var report = PoolQuotaReport.BuildPoolQuotaReport("test", items, clock);
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
        PoolQuotaReportCountsEachRecordOnce();
    }
}
