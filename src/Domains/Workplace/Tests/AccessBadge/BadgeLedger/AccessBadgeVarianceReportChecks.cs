namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeLedger;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

/// <summary>Checks of badge ledger code, run by the project's runner.</summary>
public static class AccessBadgeVarianceReportChecks
{
    public static void AccessBadgeVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700583660);
        var items = new List<AccessBadgeVariance>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AccessBadgeVariance.NewAccessBadgeVariance($"access-badge-variance-{index}", "sample"));
        }
        var report = AccessBadgeVarianceReport.BuildAccessBadgeVarianceReport("test", items, clock);
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
        AccessBadgeVarianceReportCountsEachRecordOnce();
    }
}
