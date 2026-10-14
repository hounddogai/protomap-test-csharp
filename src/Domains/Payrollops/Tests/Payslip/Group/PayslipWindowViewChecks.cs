namespace Acme.Workforce.Payrollops.Tests.Payslip.Group;

using Acme.Workforce.Payrollops.Payslip.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PayslipWindowViewChecks
{
    public static void PayslipWindowViewShowsTheStatusInCapitals()
    {
        var item = PayslipWindow.NewPayslipWindow("code-636", "name-413");
        var view = PayslipWindowView.ToPayslipWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayslipWindowView.ToPayslipWindowViews(new List<PayslipWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayslipWindowViewShowsTheStatusInCapitals();
    }
}
