namespace Acme.Workforce.Payrollops.Tests.PayRun.RunVariance;

using Acme.Workforce.Payrollops.PayRun.RunVariance;

/// <summary>Checks of run variance code, run by the project's runner.</summary>
public static class BatchedPayRunVarianceChecks
{
    public static void BatchedPayRunVarianceStartsInCreated()
    {
        var item = BatchedPayRunVariance.NewBatchedPayRunVariance("code-313", "title-46");
        Check.Equal(item.Status, BatchedPayRunVarianceStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedPayRunVarianceMovesToPublished()
    {
        var item = BatchedPayRunVariance.NewBatchedPayRunVariance("code-313", "title-46");
        try
        {
            item.MoveTo(BatchedPayRunVarianceStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedPayRunVarianceStatus.Published, "status");
    }

    public static void BatchedPayRunVarianceRejectsMovesFromRetired()
    {
        var item = BatchedPayRunVariance.NewBatchedPayRunVariance("code-313", "title-46");
        item.Status = BatchedPayRunVarianceStatus.Retired;
        Check.Fails(() => item.MoveTo(BatchedPayRunVarianceStatus.Created), "move");
    }

    public static void RunAll()
    {
        BatchedPayRunVarianceStartsInCreated();
        BatchedPayRunVarianceMovesToPublished();
        BatchedPayRunVarianceRejectsMovesFromRetired();
    }
}
