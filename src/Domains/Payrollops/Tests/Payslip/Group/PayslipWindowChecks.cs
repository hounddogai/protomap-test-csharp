namespace Acme.Workforce.Payrollops.Tests.Payslip.Group;

using Acme.Workforce.Payrollops.Payslip.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PayslipWindowChecks
{
    public static void PayslipWindowStartsInCreated()
    {
        var item = PayslipWindow.NewPayslipWindow("code-636", "name-413");
        Check.Equal(item.Status, PayslipWindowStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayslipWindowMovesToPublished()
    {
        var item = PayslipWindow.NewPayslipWindow("code-636", "name-413");
        try
        {
            item.MoveTo(PayslipWindowStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayslipWindowStatus.Published, "status");
    }

    public static void PayslipWindowRejectsMovesFromRetired()
    {
        var item = PayslipWindow.NewPayslipWindow("code-636", "name-413");
        item.Status = PayslipWindowStatus.Retired;
        Check.Fails(() => item.MoveTo(PayslipWindowStatus.Created), "move");
    }

    public static void RunAll()
    {
        PayslipWindowStartsInCreated();
        PayslipWindowMovesToPublished();
        PayslipWindowRejectsMovesFromRetired();
    }
}
