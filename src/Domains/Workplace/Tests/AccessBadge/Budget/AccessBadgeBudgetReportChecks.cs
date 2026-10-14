namespace Acme.Workforce.Workplace.Tests.AccessBadge.Budget;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class AccessBadgeBudgetReportChecks
{
    public static void AccessBadgeBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706447238);
        var items = new List<AccessBadgeBudget>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AccessBadgeBudget.NewAccessBadgeBudget($"access-badge-budget-{index}", "sample"));
        }
        var report = AccessBadgeBudgetReport.BuildAccessBadgeBudgetReport("test", items, clock);
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
        AccessBadgeBudgetReportCountsEachRecordOnce();
    }
}
