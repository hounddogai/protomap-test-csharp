namespace Acme.Workforce.Workplace.Tests.AccessBadge.Ledger;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class DefaultAccessBadgeBundleReportChecks
{
    public static void DefaultAccessBadgeBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704621684);
        var items = new List<DefaultAccessBadgeBundle>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(DefaultAccessBadgeBundle.NewDefaultAccessBadgeBundle($"default-access-badge-bundle-{index}", "sample"));
        }
        var report = DefaultAccessBadgeBundleReport.BuildDefaultAccessBadgeBundleReport("test", items, clock);
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
        DefaultAccessBadgeBundleReportCountsEachRecordOnce();
    }
}
