namespace Acme.Workforce.Payrollops.Tests.Payslip.Threshold;

using Acme.Workforce.Payrollops.Payslip.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class StrictPayslipOverrideChecks
{
    public static void StrictPayslipOverrideStartsInOpen()
    {
        var item = StrictPayslipOverride.NewStrictPayslipOverride("code-870", "title-956");
        Check.Equal(item.Status, StrictPayslipOverrideStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictPayslipOverrideMovesToReopened()
    {
        var item = StrictPayslipOverride.NewStrictPayslipOverride("code-870", "title-956");
        try
        {
            item.MoveTo(StrictPayslipOverrideStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictPayslipOverrideStatus.Reopened, "status");
    }

    public static void StrictPayslipOverrideRejectsMovesFromArchived()
    {
        var item = StrictPayslipOverride.NewStrictPayslipOverride("code-870", "title-956");
        item.Status = StrictPayslipOverrideStatus.Archived;
        Check.Fails(() => item.MoveTo(StrictPayslipOverrideStatus.Open), "move");
    }

    public static void RunAll()
    {
        StrictPayslipOverrideStartsInOpen();
        StrictPayslipOverrideMovesToReopened();
        StrictPayslipOverrideRejectsMovesFromArchived();
    }
}
