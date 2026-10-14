namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipNotice;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Payslip.PayslipNotice;

/// <summary>Checks of payslip notice code, run by the project's runner.</summary>
public static class PayslipBudgetReportChecks
{
    public static void PayslipBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707484507);
        var items = new List<PayslipBudget>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PayslipBudget.NewPayslipBudget($"payslip-budget-{index}", "sample"));
        }
        var report = PayslipBudgetReport.BuildPayslipBudgetReport("test", items, clock);
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
        PayslipBudgetReportCountsEachRecordOnce();
    }
}
