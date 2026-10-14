namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipNotice;

using Acme.Workforce.Payrollops.Payslip.PayslipNotice;

/// <summary>Checks of payslip notice code, run by the project's runner.</summary>
public static class ScheduledPayslipNoticeViewChecks
{
    public static void ScheduledPayslipNoticeViewShowsTheStatusInCapitals()
    {
        var item = ScheduledPayslipNotice.NewScheduledPayslipNotice("code-328", "name-891");
        var view = ScheduledPayslipNoticeView.ToScheduledPayslipNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledPayslipNoticeView.ToScheduledPayslipNoticeViews(new List<ScheduledPayslipNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledPayslipNoticeViewShowsTheStatusInCapitals();
    }
}
