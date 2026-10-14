namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipAssignment;

using Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

/// <summary>Checks of payslip assignment code, run by the project's runner.</summary>
public static class LenientPayslipRollupChecks
{
    public static void LenientPayslipRollupStartsInCreated()
    {
        var item = LenientPayslipRollup.NewLenientPayslipRollup("code-887", "title-377");
        Check.Equal(item.Status, LenientPayslipRollupStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientPayslipRollupMovesToRetired()
    {
        var item = LenientPayslipRollup.NewLenientPayslipRollup("code-887", "title-377");
        try
        {
            item.MoveTo(LenientPayslipRollupStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientPayslipRollupStatus.Retired, "status");
    }

    public static void LenientPayslipRollupRejectsMovesFromRetired()
    {
        var item = LenientPayslipRollup.NewLenientPayslipRollup("code-887", "title-377");
        item.Status = LenientPayslipRollupStatus.Retired;
        Check.Fails(() => item.MoveTo(LenientPayslipRollupStatus.Created), "move");
    }

    public static void RunAll()
    {
        LenientPayslipRollupStartsInCreated();
        LenientPayslipRollupMovesToRetired();
        LenientPayslipRollupRejectsMovesFromRetired();
    }
}
