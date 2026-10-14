namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeBudget;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

/// <summary>Checks of badge budget code, run by the project's runner.</summary>
public static class DefaultAccessBadgeRollupReportChecks
{
    public static void DefaultAccessBadgeRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706197006);
        var items = new List<DefaultAccessBadgeRollup>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(DefaultAccessBadgeRollup.NewDefaultAccessBadgeRollup($"default-access-badge-rollup-{index}", "sample"));
        }
        var report = DefaultAccessBadgeRollupReport.BuildDefaultAccessBadgeRollupReport("test", items, clock);
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
        DefaultAccessBadgeRollupReportCountsEachRecordOnce();
    }
}
