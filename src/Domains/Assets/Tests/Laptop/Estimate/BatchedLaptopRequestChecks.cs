namespace Acme.Workforce.Assets.Tests.Laptop.Estimate;

using Acme.Workforce.Assets.Laptop.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class BatchedLaptopRequestChecks
{
    public static void BatchedLaptopRequestStartsInPlanned()
    {
        var item = BatchedLaptopRequest.NewBatchedLaptopRequest("code-336", "label-114");
        Check.Equal(item.Status, BatchedLaptopRequestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedLaptopRequestMovesToInProgress()
    {
        var item = BatchedLaptopRequest.NewBatchedLaptopRequest("code-336", "label-114");
        try
        {
            item.MoveTo(BatchedLaptopRequestStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedLaptopRequestStatus.InProgress, "status");
    }

    public static void BatchedLaptopRequestRejectsMovesFromCancelled()
    {
        var item = BatchedLaptopRequest.NewBatchedLaptopRequest("code-336", "label-114");
        item.Status = BatchedLaptopRequestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(BatchedLaptopRequestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        BatchedLaptopRequestStartsInPlanned();
        BatchedLaptopRequestMovesToInProgress();
        BatchedLaptopRequestRejectsMovesFromCancelled();
    }
}
