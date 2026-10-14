namespace Acme.Workforce.Payrollops.Tests.Payslip.Budget;

using Acme.Workforce.Payrollops.Payslip.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class ManualPayslipEstimateChecks
{
    public static void ManualPayslipEstimateStartsInDraft()
    {
        var item = ManualPayslipEstimate.NewManualPayslipEstimate("code-223", "title-689");
        Check.Equal(item.Status, ManualPayslipEstimateStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualPayslipEstimateMovesToPending()
    {
        var item = ManualPayslipEstimate.NewManualPayslipEstimate("code-223", "title-689");
        try
        {
            item.MoveTo(ManualPayslipEstimateStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualPayslipEstimateStatus.Pending, "status");
    }

    public static void ManualPayslipEstimateRejectsMovesFromClosed()
    {
        var item = ManualPayslipEstimate.NewManualPayslipEstimate("code-223", "title-689");
        item.Status = ManualPayslipEstimateStatus.Closed;
        Check.Fails(() => item.MoveTo(ManualPayslipEstimateStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ManualPayslipEstimateStartsInDraft();
        ManualPayslipEstimateMovesToPending();
        ManualPayslipEstimateRejectsMovesFromClosed();
    }
}
