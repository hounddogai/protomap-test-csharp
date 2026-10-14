namespace Acme.Workforce.Payrollops.Tests.Payslip.Snapshot;

using Acme.Workforce.Payrollops.Payslip.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class PayslipSnapshotViewChecks
{
    public static void PayslipSnapshotViewShowsTheStatusInCapitals()
    {
        var item = PayslipSnapshot.NewPayslipSnapshot("code-677", "label-667");
        var view = PayslipSnapshotView.ToPayslipSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayslipSnapshotView.ToPayslipSnapshotViews(new List<PayslipSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayslipSnapshotViewShowsTheStatusInCapitals();
    }
}
