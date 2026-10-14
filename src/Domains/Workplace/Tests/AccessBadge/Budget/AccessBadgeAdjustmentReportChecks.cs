namespace Acme.Workforce.Workplace.Tests.AccessBadge.Budget;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class AccessBadgeAdjustmentReportChecks
{
    public static void AccessBadgeAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709955302);
        var items = new List<AccessBadgeAdjustment>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AccessBadgeAdjustment.NewAccessBadgeAdjustment($"access-badge-adjustment-{index}", "sample"));
        }
        var report = AccessBadgeAdjustmentReport.BuildAccessBadgeAdjustmentReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        AccessBadgeAdjustmentReportCountsEachRecordOnce();
    }
}
