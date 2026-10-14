namespace Acme.Workforce.Payrollops.Tests.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

/// <summary>Checks of rolling payslip snapshot code, run by the project's runner.</summary>
public static class PrimaryPayslipSnapshotChecks
{
    public static void PrimaryPayslipSnapshotStartsInOpen()
    {
        var item = PrimaryPayslipSnapshot.NewPrimaryPayslipSnapshot("code-995", "label-152");
        Check.Equal(item.Status, PrimaryPayslipSnapshotStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryPayslipSnapshotMovesToInReview()
    {
        var item = PrimaryPayslipSnapshot.NewPrimaryPayslipSnapshot("code-995", "label-152");
        try
        {
            item.MoveTo(PrimaryPayslipSnapshotStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryPayslipSnapshotStatus.InReview, "status");
    }

    public static void PrimaryPayslipSnapshotRejectsMovesFromArchived()
    {
        var item = PrimaryPayslipSnapshot.NewPrimaryPayslipSnapshot("code-995", "label-152");
        item.Status = PrimaryPayslipSnapshotStatus.Archived;
        Check.Fails(() => item.MoveTo(PrimaryPayslipSnapshotStatus.Open), "move");
    }

    public static void RunAll()
    {
        PrimaryPayslipSnapshotStartsInOpen();
        PrimaryPayslipSnapshotMovesToInReview();
        PrimaryPayslipSnapshotRejectsMovesFromArchived();
    }
}
