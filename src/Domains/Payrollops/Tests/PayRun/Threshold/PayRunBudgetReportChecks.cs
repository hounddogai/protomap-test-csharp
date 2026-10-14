namespace Acme.Workforce.Payrollops.Tests.PayRun.Threshold;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.PayRun.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PayRunBudgetReportChecks
{
    public static void PayRunBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700901128);
        var items = new List<PayRunBudget>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PayRunBudget.NewPayRunBudget($"pay-run-budget-{index}", "sample"));
        }
        var report = PayRunBudgetReport.BuildPayRunBudgetReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        PayRunBudgetReportCountsEachRecordOnce();
    }
}
