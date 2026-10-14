namespace Acme.Workforce.Payrollops.Tests.Payslip.Manifest;

using Acme.Workforce.Payrollops.Payslip.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class PayslipEntryChecks
{
    public static void PayslipEntryStartsInProposed()
    {
        var item = PayslipEntry.NewPayslipEntry("code-723", "title-119");
        Check.Equal(item.Status, PayslipEntryStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayslipEntryMovesToWithdrawn()
    {
        var item = PayslipEntry.NewPayslipEntry("code-723", "title-119");
        try
        {
            item.MoveTo(PayslipEntryStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayslipEntryStatus.Withdrawn, "status");
    }

    public static void PayslipEntryRejectsMovesFromWithdrawn()
    {
        var item = PayslipEntry.NewPayslipEntry("code-723", "title-119");
        item.Status = PayslipEntryStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PayslipEntryStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PayslipEntryStartsInProposed();
        PayslipEntryMovesToWithdrawn();
        PayslipEntryRejectsMovesFromWithdrawn();
    }
}
