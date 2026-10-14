namespace Acme.Workforce.Payrollops.Tests.Payslip.Threshold;

using Acme.Workforce.Payrollops.Payslip.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class ScheduledPayslipAllocationViewChecks
{
    public static void ScheduledPayslipAllocationViewShowsTheStatusInCapitals()
    {
        var item = ScheduledPayslipAllocation.NewScheduledPayslipAllocation("code-583", "label-712");
        var view = ScheduledPayslipAllocationView.ToScheduledPayslipAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledPayslipAllocationView.ToScheduledPayslipAllocationViews(new List<ScheduledPayslipAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledPayslipAllocationViewShowsTheStatusInCapitals();
    }
}
