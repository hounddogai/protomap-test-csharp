namespace Acme.Workforce.Compensation.Tests.EquityGrant.Summary;

using Acme.Workforce.Compensation.EquityGrant.Summary;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class EquityGrantBudgetReportChecks
{
    public static void EquityGrantBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705960888);
        var items = new List<EquityGrantBudget>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(EquityGrantBudget.NewEquityGrantBudget($"equity-grant-budget-{index}", "sample"));
        }
        var report = EquityGrantBudgetReport.BuildEquityGrantBudgetReport("test", items, clock);
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
        EquityGrantBudgetReportCountsEachRecordOnce();
    }
}
