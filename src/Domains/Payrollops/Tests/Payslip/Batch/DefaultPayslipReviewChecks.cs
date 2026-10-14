namespace Acme.Workforce.Payrollops.Tests.Payslip.Batch;

using Acme.Workforce.Payrollops.Payslip.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class DefaultPayslipReviewChecks
{
    public static void DefaultPayslipReviewStartsInProposed()
    {
        var item = DefaultPayslipReview.NewDefaultPayslipReview("code-641", "title-543");
        Check.Equal(item.Status, DefaultPayslipReviewStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultPayslipReviewMovesToRejected()
    {
        var item = DefaultPayslipReview.NewDefaultPayslipReview("code-641", "title-543");
        try
        {
            item.MoveTo(DefaultPayslipReviewStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultPayslipReviewStatus.Rejected, "status");
    }

    public static void DefaultPayslipReviewRejectsMovesFromWithdrawn()
    {
        var item = DefaultPayslipReview.NewDefaultPayslipReview("code-641", "title-543");
        item.Status = DefaultPayslipReviewStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(DefaultPayslipReviewStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        DefaultPayslipReviewStartsInProposed();
        DefaultPayslipReviewMovesToRejected();
        DefaultPayslipReviewRejectsMovesFromWithdrawn();
    }
}
