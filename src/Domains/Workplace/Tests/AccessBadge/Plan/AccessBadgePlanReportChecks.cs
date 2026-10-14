namespace Acme.Workforce.Workplace.Tests.AccessBadge.Plan;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class AccessBadgePlanReportChecks
{
    public static void AccessBadgePlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708048666);
        var items = new List<AccessBadgePlan>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AccessBadgePlan.NewAccessBadgePlan($"access-badge-plan-{index}", "sample"));
        }
        var report = AccessBadgePlanReport.BuildAccessBadgePlanReport("test", items, clock);
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
        AccessBadgePlanReportCountsEachRecordOnce();
    }
}
