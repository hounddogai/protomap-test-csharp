namespace Acme.Workforce.Payrollops.Tests.Payslip.Group;

using Acme.Workforce.Payrollops.Payslip.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PayslipSegmentViewChecks
{
    public static void PayslipSegmentViewShowsTheStatusInCapitals()
    {
        var item = PayslipSegment.NewPayslipSegment("code-948", "title-328");
        var view = PayslipSegmentView.ToPayslipSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayslipSegmentView.ToPayslipSegmentViews(new List<PayslipSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayslipSegmentViewShowsTheStatusInCapitals();
    }
}
