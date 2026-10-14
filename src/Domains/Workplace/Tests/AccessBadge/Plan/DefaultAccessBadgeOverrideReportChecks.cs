namespace Acme.Workforce.Workplace.Tests.AccessBadge.Plan;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class DefaultAccessBadgeOverrideReportChecks
{
    public static void DefaultAccessBadgeOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709529981);
        var items = new List<DefaultAccessBadgeOverride>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(DefaultAccessBadgeOverride.NewDefaultAccessBadgeOverride($"default-access-badge-override-{index}", "sample"));
        }
        var report = DefaultAccessBadgeOverrideReport.BuildDefaultAccessBadgeOverrideReport("test", items, clock);
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
        DefaultAccessBadgeOverrideReportCountsEachRecordOnce();
    }
}
