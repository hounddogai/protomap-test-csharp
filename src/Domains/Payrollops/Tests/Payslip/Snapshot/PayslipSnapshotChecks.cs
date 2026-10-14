namespace Acme.Workforce.Payrollops.Tests.Payslip.Snapshot;

using Acme.Workforce.Payrollops.Payslip.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class PayslipSnapshotChecks
{
    public static void PayslipSnapshotStartsInNew()
    {
        var item = PayslipSnapshot.NewPayslipSnapshot("code-677", "label-667");
        Check.Equal(item.Status, PayslipSnapshotStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayslipSnapshotMovesToQueued()
    {
        var item = PayslipSnapshot.NewPayslipSnapshot("code-677", "label-667");
        try
        {
            item.MoveTo(PayslipSnapshotStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayslipSnapshotStatus.Queued, "status");
    }

    public static void PayslipSnapshotRejectsMovesFromFailed()
    {
        var item = PayslipSnapshot.NewPayslipSnapshot("code-677", "label-667");
        item.Status = PayslipSnapshotStatus.Failed;
        Check.Fails(() => item.MoveTo(PayslipSnapshotStatus.New), "move");
    }

    public static void RunAll()
    {
        PayslipSnapshotStartsInNew();
        PayslipSnapshotMovesToQueued();
        PayslipSnapshotRejectsMovesFromFailed();
    }
}
