namespace Acme.Workforce.Payrollops.Tests.RetroPay.PayBudget;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.PayBudget;

/// <summary>Checks of pay budget code, run by the project's runner.</summary>
public static class ManualRetroPayBudgetReportChecks
{
    public static void ManualRetroPayBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708648288);
        var items = new List<ManualRetroPayBudget>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ManualRetroPayBudget.NewManualRetroPayBudget($"manual-retro-pay-budget-{index}", "sample"));
        }
        var report = ManualRetroPayBudgetReport.BuildManualRetroPayBudgetReport("test", items, clock);
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
        ManualRetroPayBudgetReportCountsEachRecordOnce();
    }
}
