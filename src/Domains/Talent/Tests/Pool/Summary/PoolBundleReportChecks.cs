namespace Acme.Workforce.Talent.Tests.Pool.Summary;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PoolBundleReportChecks
{
    public static void PoolBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705017679);
        var items = new List<PoolBundle>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PoolBundle.NewPoolBundle($"pool-bundle-{index}", "sample"));
        }
        var report = PoolBundleReport.BuildPoolBundleReport("test", items, clock);
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
        PoolBundleReportCountsEachRecordOnce();
    }
}
