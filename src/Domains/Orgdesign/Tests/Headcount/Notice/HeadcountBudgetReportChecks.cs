namespace Acme.Workforce.Orgdesign.Tests.Headcount.Notice;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Headcount.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class HeadcountBudgetReportChecks
{
    public static void HeadcountBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700725287);
        var items = new List<HeadcountBudget>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(HeadcountBudget.NewHeadcountBudget($"headcount-budget-{index}", "sample"));
        }
        var report = HeadcountBudgetReport.BuildHeadcountBudgetReport("test", items, clock);
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
        HeadcountBudgetReportCountsEachRecordOnce();
    }
}
