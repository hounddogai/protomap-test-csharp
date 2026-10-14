namespace Acme.Workforce.Payrollops.Tests.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

/// <summary>Checks of rolling payslip snapshot code, run by the project's runner.</summary>
public static class AdaptivePayslipClaimChecks
{
    public static void AdaptivePayslipClaimStartsInNew()
    {
        var item = AdaptivePayslipClaim.NewAdaptivePayslipClaim("code-291", "label-749");
        Check.Equal(item.Status, AdaptivePayslipClaimStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptivePayslipClaimMovesToDone()
    {
        var item = AdaptivePayslipClaim.NewAdaptivePayslipClaim("code-291", "label-749");
        try
        {
            item.MoveTo(AdaptivePayslipClaimStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptivePayslipClaimStatus.Done, "status");
    }

    public static void AdaptivePayslipClaimRejectsMovesFromFailed()
    {
        var item = AdaptivePayslipClaim.NewAdaptivePayslipClaim("code-291", "label-749");
        item.Status = AdaptivePayslipClaimStatus.Failed;
        Check.Fails(() => item.MoveTo(AdaptivePayslipClaimStatus.New), "move");
    }

    public static void RunAll()
    {
        AdaptivePayslipClaimStartsInNew();
        AdaptivePayslipClaimMovesToDone();
        AdaptivePayslipClaimRejectsMovesFromFailed();
    }
}
