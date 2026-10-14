namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipNotice;

using Acme.Workforce.Payrollops.Payslip.PayslipNotice;

/// <summary>Checks of payslip notice code, run by the project's runner.</summary>
public static class BatchedPayslipClaimChecks
{
    public static void BatchedPayslipClaimStartsInRequested()
    {
        var item = BatchedPayslipClaim.NewBatchedPayslipClaim("code-857", "name-550");
        Check.Equal(item.Status, BatchedPayslipClaimStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedPayslipClaimMovesToGranted()
    {
        var item = BatchedPayslipClaim.NewBatchedPayslipClaim("code-857", "name-550");
        try
        {
            item.MoveTo(BatchedPayslipClaimStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedPayslipClaimStatus.Granted, "status");
    }

    public static void BatchedPayslipClaimRejectsMovesFromExpired()
    {
        var item = BatchedPayslipClaim.NewBatchedPayslipClaim("code-857", "name-550");
        item.Status = BatchedPayslipClaimStatus.Expired;
        Check.Fails(() => item.MoveTo(BatchedPayslipClaimStatus.Requested), "move");
    }

    public static void RunAll()
    {
        BatchedPayslipClaimStartsInRequested();
        BatchedPayslipClaimMovesToGranted();
        BatchedPayslipClaimRejectsMovesFromExpired();
    }
}
