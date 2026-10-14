namespace Acme.Workforce.Engagement.Tests.TownHall.Notice;

using Acme.Workforce.Engagement.TownHall.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class BatchedTownHallExceptionChecks
{
    public static void BatchedTownHallExceptionStartsInDraft()
    {
        var item = BatchedTownHallException.NewBatchedTownHallException("code-424", "name-187");
        Check.Equal(item.Status, BatchedTownHallExceptionStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedTownHallExceptionMovesToPending()
    {
        var item = BatchedTownHallException.NewBatchedTownHallException("code-424", "name-187");
        try
        {
            item.MoveTo(BatchedTownHallExceptionStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedTownHallExceptionStatus.Pending, "status");
    }

    public static void BatchedTownHallExceptionRejectsMovesFromClosed()
    {
        var item = BatchedTownHallException.NewBatchedTownHallException("code-424", "name-187");
        item.Status = BatchedTownHallExceptionStatus.Closed;
        Check.Fails(() => item.MoveTo(BatchedTownHallExceptionStatus.Draft), "move");
    }

    public static void RunAll()
    {
        BatchedTownHallExceptionStartsInDraft();
        BatchedTownHallExceptionMovesToPending();
        BatchedTownHallExceptionRejectsMovesFromClosed();
    }
}
