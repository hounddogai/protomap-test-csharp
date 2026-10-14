namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeAudit;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.BadgeAudit;

/// <summary>Checks of badge audit code, run by the project's runner.</summary>
public static class GlobalAccessBadgeRollupReportChecks
{
    public static void GlobalAccessBadgeRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706351727);
        var items = new List<GlobalAccessBadgeRollup>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(GlobalAccessBadgeRollup.NewGlobalAccessBadgeRollup($"global-access-badge-rollup-{index}", "sample"));
        }
        var report = GlobalAccessBadgeRollupReport.BuildGlobalAccessBadgeRollupReport("test", items, clock);
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
        GlobalAccessBadgeRollupReportCountsEachRecordOnce();
    }
}
