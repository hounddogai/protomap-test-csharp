namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipAssignment;

using Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

/// <summary>Checks of payslip assignment code, run by the project's runner.</summary>
public static class FallbackPayslipCheckpointViewChecks
{
    public static void FallbackPayslipCheckpointViewShowsTheStatusInCapitals()
    {
        var item = FallbackPayslipCheckpoint.NewFallbackPayslipCheckpoint("code-48", "name-255");
        var view = FallbackPayslipCheckpointView.ToFallbackPayslipCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackPayslipCheckpointView.ToFallbackPayslipCheckpointViews(new List<FallbackPayslipCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackPayslipCheckpointViewShowsTheStatusInCapitals();
    }
}
