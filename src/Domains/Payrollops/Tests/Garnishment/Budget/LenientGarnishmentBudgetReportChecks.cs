namespace Acme.Workforce.Payrollops.Tests.Garnishment.Budget;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Garnishment.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class LenientGarnishmentBudgetReportChecks
{
    public static void LenientGarnishmentBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700446985);
        var items = new List<LenientGarnishmentBudget>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LenientGarnishmentBudget.NewLenientGarnishmentBudget($"lenient-garnishment-budget-{index}", "sample"));
        }
        var report = LenientGarnishmentBudgetReport.BuildLenientGarnishmentBudgetReport("test", items, clock);
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
        LenientGarnishmentBudgetReportCountsEachRecordOnce();
    }
}
