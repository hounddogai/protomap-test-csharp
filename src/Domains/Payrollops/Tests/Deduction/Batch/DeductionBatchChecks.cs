namespace Acme.Workforce.Payrollops.Tests.Deduction.Batch;

using Acme.Workforce.Payrollops.Deduction.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class DeductionBatchChecks
{
    public static void DeductionBatchStartsInOpen()
    {
        var item = DeductionBatch.NewDeductionBatch("code-654", "label-135");
        Check.Equal(item.Status, DeductionBatchStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionBatchMovesToInReview()
    {
        var item = DeductionBatch.NewDeductionBatch("code-654", "label-135");
        try
        {
            item.MoveTo(DeductionBatchStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionBatchStatus.InReview, "status");
    }

    public static void DeductionBatchRejectsMovesFromArchived()
    {
        var item = DeductionBatch.NewDeductionBatch("code-654", "label-135");
        item.Status = DeductionBatchStatus.Archived;
        Check.Fails(() => item.MoveTo(DeductionBatchStatus.Open), "move");
    }

    public static void RunAll()
    {
        DeductionBatchStartsInOpen();
        DeductionBatchMovesToInReview();
        DeductionBatchRejectsMovesFromArchived();
    }
}
