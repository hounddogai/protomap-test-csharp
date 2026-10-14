namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyAllocation;

using Acme.Workforce.Assets.Warranty.WarrantyAllocation;

/// <summary>Checks of warranty allocation code, run by the project's runner.</summary>
public static class BatchedWarrantyDraftChecks
{
    public static void BatchedWarrantyDraftStartsInIdle()
    {
        var item = BatchedWarrantyDraft.NewBatchedWarrantyDraft("code-626", "title-996");
        Check.Equal(item.Status, BatchedWarrantyDraftStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedWarrantyDraftMovesToReady()
    {
        var item = BatchedWarrantyDraft.NewBatchedWarrantyDraft("code-626", "title-996");
        try
        {
            item.MoveTo(BatchedWarrantyDraftStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedWarrantyDraftStatus.Ready, "status");
    }

    public static void BatchedWarrantyDraftRejectsMovesFromStopped()
    {
        var item = BatchedWarrantyDraft.NewBatchedWarrantyDraft("code-626", "title-996");
        item.Status = BatchedWarrantyDraftStatus.Stopped;
        Check.Fails(() => item.MoveTo(BatchedWarrantyDraftStatus.Idle), "move");
    }

    public static void RunAll()
    {
        BatchedWarrantyDraftStartsInIdle();
        BatchedWarrantyDraftMovesToReady();
        BatchedWarrantyDraftRejectsMovesFromStopped();
    }
}
