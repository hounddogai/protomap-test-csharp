namespace Acme.Workforce.Contractors.Tests.Engagement.Snapshot;

using Acme.Workforce.Contractors.Engagement.Snapshot;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class EngagementBudgetReportChecks
{
    public static void EngagementBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702499884);
        var items = new List<EngagementBudget>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(EngagementBudget.NewEngagementBudget($"engagement-budget-{index}", "sample"));
        }
        var report = EngagementBudgetReport.BuildEngagementBudgetReport("test", items, clock);
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
        EngagementBudgetReportCountsEachRecordOnce();
    }
}
