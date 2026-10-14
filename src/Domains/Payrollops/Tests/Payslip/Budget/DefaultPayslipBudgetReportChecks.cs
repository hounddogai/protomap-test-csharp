namespace Acme.Workforce.Payrollops.Tests.Payslip.Budget;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Payslip.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class DefaultPayslipBudgetReportChecks
{
    public static void DefaultPayslipBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705803792);
        var items = new List<DefaultPayslipBudget>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(DefaultPayslipBudget.NewDefaultPayslipBudget($"default-payslip-budget-{index}", "sample"));
        }
        var report = DefaultPayslipBudgetReport.BuildDefaultPayslipBudgetReport("test", items, clock);
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
        DefaultPayslipBudgetReportCountsEachRecordOnce();
    }
}
