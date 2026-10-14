namespace Acme.Workforce.Payrollops.Tests.Payslip.Claim;

using Acme.Workforce.Payrollops.Payslip.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PayslipBaselineChecks
{
    public static void PayslipBaselineStartsInPlanned()
    {
        var item = PayslipBaseline.NewPayslipBaseline("code-7", "title-931");
        Check.Equal(item.Status, PayslipBaselineStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayslipBaselineMovesToInProgress()
    {
        var item = PayslipBaseline.NewPayslipBaseline("code-7", "title-931");
        try
        {
            item.MoveTo(PayslipBaselineStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayslipBaselineStatus.InProgress, "status");
    }

    public static void PayslipBaselineRejectsMovesFromCancelled()
    {
        var item = PayslipBaseline.NewPayslipBaseline("code-7", "title-931");
        item.Status = PayslipBaselineStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PayslipBaselineStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PayslipBaselineStartsInPlanned();
        PayslipBaselineMovesToInProgress();
        PayslipBaselineRejectsMovesFromCancelled();
    }
}
