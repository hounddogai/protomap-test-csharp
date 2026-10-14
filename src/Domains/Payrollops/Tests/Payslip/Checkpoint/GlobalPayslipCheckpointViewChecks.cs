namespace Acme.Workforce.Payrollops.Tests.Payslip.Checkpoint;

using Acme.Workforce.Payrollops.Payslip.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class GlobalPayslipCheckpointViewChecks
{
    public static void GlobalPayslipCheckpointViewShowsTheStatusInCapitals()
    {
        var item = GlobalPayslipCheckpoint.NewGlobalPayslipCheckpoint("code-107", "name-675");
        var view = GlobalPayslipCheckpointView.ToGlobalPayslipCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalPayslipCheckpointView.ToGlobalPayslipCheckpointViews(new List<GlobalPayslipCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalPayslipCheckpointViewShowsTheStatusInCapitals();
    }
}
