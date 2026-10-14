namespace Acme.Workforce.Payrollops.Tests.Payslip.Snapshot;

using Acme.Workforce.Payrollops.Payslip.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class GlobalPayslipNoticeViewChecks
{
    public static void GlobalPayslipNoticeViewShowsTheStatusInCapitals()
    {
        var item = GlobalPayslipNotice.NewGlobalPayslipNotice("code-807", "name-390");
        var view = GlobalPayslipNoticeView.ToGlobalPayslipNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalPayslipNoticeView.ToGlobalPayslipNoticeViews(new List<GlobalPayslipNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalPayslipNoticeViewShowsTheStatusInCapitals();
    }
}
