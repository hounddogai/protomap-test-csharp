namespace Acme.Workforce.Payrollops.Tests.RetroPay.PaySummary;

using Acme.Workforce.Payrollops.RetroPay.PaySummary;

/// <summary>Checks of pay summary code, run by the project's runner.</summary>
public static class BatchedRetroPayEstimateChecks
{
    public static void BatchedRetroPayEstimateStartsInCreated()
    {
        var item = BatchedRetroPayEstimate.NewBatchedRetroPayEstimate("code-668", "title-90");
        Check.Equal(item.Status, BatchedRetroPayEstimateStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedRetroPayEstimateMovesToPublished()
    {
        var item = BatchedRetroPayEstimate.NewBatchedRetroPayEstimate("code-668", "title-90");
        try
        {
            item.MoveTo(BatchedRetroPayEstimateStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedRetroPayEstimateStatus.Published, "status");
    }

    public static void BatchedRetroPayEstimateRejectsMovesFromRetired()
    {
        var item = BatchedRetroPayEstimate.NewBatchedRetroPayEstimate("code-668", "title-90");
        item.Status = BatchedRetroPayEstimateStatus.Retired;
        Check.Fails(() => item.MoveTo(BatchedRetroPayEstimateStatus.Created), "move");
    }

    public static void RunAll()
    {
        BatchedRetroPayEstimateStartsInCreated();
        BatchedRetroPayEstimateMovesToPublished();
        BatchedRetroPayEstimateRejectsMovesFromRetired();
    }
}
