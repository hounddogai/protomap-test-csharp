namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipSnapshot;

using Acme.Workforce.Payrollops.Payslip.PayslipSnapshot;

/// <summary>Checks of payslip snapshot code, run by the project's runner.</summary>
public static class PayslipTargetChecks
{
    public static void PayslipTargetStartsInNew()
    {
        var item = PayslipTarget.NewPayslipTarget("code-168", "label-501");
        Check.Equal(item.Status, PayslipTargetStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayslipTargetMovesToQueued()
    {
        var item = PayslipTarget.NewPayslipTarget("code-168", "label-501");
        try
        {
            item.MoveTo(PayslipTargetStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayslipTargetStatus.Queued, "status");
    }

    public static void PayslipTargetRejectsMovesFromFailed()
    {
        var item = PayslipTarget.NewPayslipTarget("code-168", "label-501");
        item.Status = PayslipTargetStatus.Failed;
        Check.Fails(() => item.MoveTo(PayslipTargetStatus.New), "move");
    }

    public static void RunAll()
    {
        PayslipTargetStartsInNew();
        PayslipTargetMovesToQueued();
        PayslipTargetRejectsMovesFromFailed();
    }
}
