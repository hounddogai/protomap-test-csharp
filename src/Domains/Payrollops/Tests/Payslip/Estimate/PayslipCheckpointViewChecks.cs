namespace Acme.Workforce.Payrollops.Tests.Payslip.Estimate;

using Acme.Workforce.Payrollops.Payslip.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PayslipCheckpointViewChecks
{
    public static void PayslipCheckpointViewShowsTheStatusInCapitals()
    {
        var item = PayslipCheckpoint.NewPayslipCheckpoint("code-134", "name-543");
        var view = PayslipCheckpointView.ToPayslipCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayslipCheckpointView.ToPayslipCheckpointViews(new List<PayslipCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayslipCheckpointViewShowsTheStatusInCapitals();
    }
}
