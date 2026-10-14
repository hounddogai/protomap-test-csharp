namespace Acme.Workforce.Assets.Tests.Laptop.Review;

using Acme.Workforce.Assets.Laptop.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class BatchedLaptopVarianceChecks
{
    public static void BatchedLaptopVarianceStartsInProposed()
    {
        var item = BatchedLaptopVariance.NewBatchedLaptopVariance("code-421", "title-903");
        Check.Equal(item.Status, BatchedLaptopVarianceStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedLaptopVarianceMovesToApproved()
    {
        var item = BatchedLaptopVariance.NewBatchedLaptopVariance("code-421", "title-903");
        try
        {
            item.MoveTo(BatchedLaptopVarianceStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedLaptopVarianceStatus.Approved, "status");
    }

    public static void BatchedLaptopVarianceRejectsMovesFromWithdrawn()
    {
        var item = BatchedLaptopVariance.NewBatchedLaptopVariance("code-421", "title-903");
        item.Status = BatchedLaptopVarianceStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(BatchedLaptopVarianceStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        BatchedLaptopVarianceStartsInProposed();
        BatchedLaptopVarianceMovesToApproved();
        BatchedLaptopVarianceRejectsMovesFromWithdrawn();
    }
}
