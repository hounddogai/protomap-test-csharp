namespace Acme.Workforce.Payrollops.Tests.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

/// <summary>Checks of rolling payslip snapshot code, run by the project's runner.</summary>
public static class PrimaryPayslipSnapshotViewChecks
{
    public static void PrimaryPayslipSnapshotViewShowsTheStatusInCapitals()
    {
        var item = PrimaryPayslipSnapshot.NewPrimaryPayslipSnapshot("code-995", "label-152");
        var view = PrimaryPayslipSnapshotView.ToPrimaryPayslipSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryPayslipSnapshotView.ToPrimaryPayslipSnapshotViews(new List<PrimaryPayslipSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryPayslipSnapshotViewShowsTheStatusInCapitals();
    }
}
