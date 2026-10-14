namespace Acme.Workforce.Orgdesign.Tests.Position.Segment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class FallbackPositionQuotaReportChecks
{
    public static void FallbackPositionQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704991721);
        var items = new List<FallbackPositionQuota>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(FallbackPositionQuota.NewFallbackPositionQuota($"fallback-position-quota-{index}", "sample"));
        }
        var report = FallbackPositionQuotaReport.BuildFallbackPositionQuotaReport("test", items, clock);
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
        FallbackPositionQuotaReportCountsEachRecordOnce();
    }
}
