namespace Acme.Workforce.Workplace.Tests.AccessBadge.Manifest;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class FallbackAccessBadgeOverrideReportChecks
{
    public static void FallbackAccessBadgeOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700441556);
        var items = new List<FallbackAccessBadgeOverride>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(FallbackAccessBadgeOverride.NewFallbackAccessBadgeOverride($"fallback-access-badge-override-{index}", "sample"));
        }
        var report = FallbackAccessBadgeOverrideReport.BuildFallbackAccessBadgeOverrideReport("test", items, clock);
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
        FallbackAccessBadgeOverrideReportCountsEachRecordOnce();
    }
}
