namespace Acme.Workforce.Workplace.Tests.Desk.RecordItem;

using Acme.Workforce.Workplace.Desk.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class GlobalDeskBatchChecks
{
    public static void GlobalDeskBatchStartsInNew()
    {
        var item = GlobalDeskBatch.NewGlobalDeskBatch("code-606", "label-174");
        Check.Equal(item.Status, GlobalDeskBatchStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalDeskBatchMovesToQueued()
    {
        var item = GlobalDeskBatch.NewGlobalDeskBatch("code-606", "label-174");
        try
        {
            item.MoveTo(GlobalDeskBatchStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalDeskBatchStatus.Queued, "status");
    }

    public static void GlobalDeskBatchRejectsMovesFromFailed()
    {
        var item = GlobalDeskBatch.NewGlobalDeskBatch("code-606", "label-174");
        item.Status = GlobalDeskBatchStatus.Failed;
        Check.Fails(() => item.MoveTo(GlobalDeskBatchStatus.New), "move");
    }

    public static void RunAll()
    {
        GlobalDeskBatchStartsInNew();
        GlobalDeskBatchMovesToQueued();
        GlobalDeskBatchRejectsMovesFromFailed();
    }
}
