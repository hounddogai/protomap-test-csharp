namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipNotice;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Payslip.PayslipNotice;

/// <summary>Checks of payslip notice code, run by the project's runner.</summary>
public static class ScheduledPayslipNoticeReportChecks
{
    public static void ScheduledPayslipNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702623549);
        var items = new List<ScheduledPayslipNotice>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ScheduledPayslipNotice.NewScheduledPayslipNotice($"scheduled-payslip-notice-{index}", "sample"));
        }
        var report = ScheduledPayslipNoticeReport.BuildScheduledPayslipNoticeReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        ScheduledPayslipNoticeReportCountsEachRecordOnce();
    }
}
