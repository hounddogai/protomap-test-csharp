namespace Acme.Workforce.Engagement.Tests.TownHall.Threshold;

using Acme.Workforce.Engagement.TownHall.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class BatchedTownHallNoticeChecks
{
    public static void BatchedTownHallNoticeStartsInIdle()
    {
        var item = BatchedTownHallNotice.NewBatchedTownHallNotice("code-911", "name-594");
        Check.Equal(item.Status, BatchedTownHallNoticeStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedTownHallNoticeMovesToWarming()
    {
        var item = BatchedTownHallNotice.NewBatchedTownHallNotice("code-911", "name-594");
        try
        {
            item.MoveTo(BatchedTownHallNoticeStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedTownHallNoticeStatus.Warming, "status");
    }

    public static void BatchedTownHallNoticeRejectsMovesFromStopped()
    {
        var item = BatchedTownHallNotice.NewBatchedTownHallNotice("code-911", "name-594");
        item.Status = BatchedTownHallNoticeStatus.Stopped;
        Check.Fails(() => item.MoveTo(BatchedTownHallNoticeStatus.Idle), "move");
    }

    public static void RunAll()
    {
        BatchedTownHallNoticeStartsInIdle();
        BatchedTownHallNoticeMovesToWarming();
        BatchedTownHallNoticeRejectsMovesFromStopped();
    }
}
