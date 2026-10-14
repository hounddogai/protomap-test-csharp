namespace Acme.Workforce.Orgdesign.Tests.Headcount.Target;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Headcount.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PendingHeadcountBudgetReportChecks
{
    public static void PendingHeadcountBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702498613);
        var items = new List<PendingHeadcountBudget>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PendingHeadcountBudget.NewPendingHeadcountBudget($"pending-headcount-budget-{index}", "sample"));
        }
        var report = PendingHeadcountBudgetReport.BuildPendingHeadcountBudgetReport("test", items, clock);
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
        PendingHeadcountBudgetReportCountsEachRecordOnce();
    }
}
