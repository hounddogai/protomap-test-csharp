namespace Acme.Workforce.Payrollops.Tests.Payslip.Slot;

using Acme.Workforce.Payrollops.Payslip.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class PayslipRequestViewChecks
{
    public static void PayslipRequestViewShowsTheStatusInCapitals()
    {
        var item = PayslipRequest.NewPayslipRequest("code-917", "title-894");
        var view = PayslipRequestView.ToPayslipRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayslipRequestView.ToPayslipRequestViews(new List<PayslipRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayslipRequestViewShowsTheStatusInCapitals();
    }
}
