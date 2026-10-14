namespace Acme.Workforce.Workplace.Tests.Desk.Tier;

using Acme.Workforce.Workplace.Desk.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class BatchedDeskTierChecks
{
    public static void BatchedDeskTierStartsInNew()
    {
        var item = BatchedDeskTier.NewBatchedDeskTier("code-381", "label-115");
        Check.Equal(item.Status, BatchedDeskTierStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedDeskTierMovesToDone()
    {
        var item = BatchedDeskTier.NewBatchedDeskTier("code-381", "label-115");
        try
        {
            item.MoveTo(BatchedDeskTierStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedDeskTierStatus.Done, "status");
    }

    public static void BatchedDeskTierRejectsMovesFromFailed()
    {
        var item = BatchedDeskTier.NewBatchedDeskTier("code-381", "label-115");
        item.Status = BatchedDeskTierStatus.Failed;
        Check.Fails(() => item.MoveTo(BatchedDeskTierStatus.New), "move");
    }

    public static void RunAll()
    {
        BatchedDeskTierStartsInNew();
        BatchedDeskTierMovesToDone();
        BatchedDeskTierRejectsMovesFromFailed();
    }
}
