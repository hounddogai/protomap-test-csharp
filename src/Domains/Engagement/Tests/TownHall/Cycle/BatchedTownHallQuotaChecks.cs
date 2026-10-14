namespace Acme.Workforce.Engagement.Tests.TownHall.Cycle;

using Acme.Workforce.Engagement.TownHall.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class BatchedTownHallQuotaChecks
{
    public static void BatchedTownHallQuotaStartsInNew()
    {
        var item = BatchedTownHallQuota.NewBatchedTownHallQuota("code-787", "label-729");
        Check.Equal(item.Status, BatchedTownHallQuotaStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedTownHallQuotaMovesToQueued()
    {
        var item = BatchedTownHallQuota.NewBatchedTownHallQuota("code-787", "label-729");
        try
        {
            item.MoveTo(BatchedTownHallQuotaStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedTownHallQuotaStatus.Queued, "status");
    }

    public static void BatchedTownHallQuotaRejectsMovesFromFailed()
    {
        var item = BatchedTownHallQuota.NewBatchedTownHallQuota("code-787", "label-729");
        item.Status = BatchedTownHallQuotaStatus.Failed;
        Check.Fails(() => item.MoveTo(BatchedTownHallQuotaStatus.New), "move");
    }

    public static void RunAll()
    {
        BatchedTownHallQuotaStartsInNew();
        BatchedTownHallQuotaMovesToQueued();
        BatchedTownHallQuotaRejectsMovesFromFailed();
    }
}
