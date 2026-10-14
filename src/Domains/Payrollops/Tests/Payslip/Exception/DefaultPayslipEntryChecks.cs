namespace Acme.Workforce.Payrollops.Tests.Payslip.Exception;

using Acme.Workforce.Payrollops.Payslip.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class DefaultPayslipEntryChecks
{
    public static void DefaultPayslipEntryStartsInDraft()
    {
        var item = DefaultPayslipEntry.NewDefaultPayslipEntry("code-971", "name-55");
        Check.Equal(item.Status, DefaultPayslipEntryStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultPayslipEntryMovesToSuspended()
    {
        var item = DefaultPayslipEntry.NewDefaultPayslipEntry("code-971", "name-55");
        try
        {
            item.MoveTo(DefaultPayslipEntryStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultPayslipEntryStatus.Suspended, "status");
    }

    public static void DefaultPayslipEntryRejectsMovesFromClosed()
    {
        var item = DefaultPayslipEntry.NewDefaultPayslipEntry("code-971", "name-55");
        item.Status = DefaultPayslipEntryStatus.Closed;
        Check.Fails(() => item.MoveTo(DefaultPayslipEntryStatus.Draft), "move");
    }

    public static void RunAll()
    {
        DefaultPayslipEntryStartsInDraft();
        DefaultPayslipEntryMovesToSuspended();
        DefaultPayslipEntryRejectsMovesFromClosed();
    }
}
