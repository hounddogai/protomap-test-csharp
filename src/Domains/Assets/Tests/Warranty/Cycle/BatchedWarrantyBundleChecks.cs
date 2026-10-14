namespace Acme.Workforce.Assets.Tests.Warranty.Cycle;

using Acme.Workforce.Assets.Warranty.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class BatchedWarrantyBundleChecks
{
    public static void BatchedWarrantyBundleStartsInNew()
    {
        var item = BatchedWarrantyBundle.NewBatchedWarrantyBundle("code-774", "name-656");
        Check.Equal(item.Status, BatchedWarrantyBundleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedWarrantyBundleMovesToQueued()
    {
        var item = BatchedWarrantyBundle.NewBatchedWarrantyBundle("code-774", "name-656");
        try
        {
            item.MoveTo(BatchedWarrantyBundleStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedWarrantyBundleStatus.Queued, "status");
    }

    public static void BatchedWarrantyBundleRejectsMovesFromFailed()
    {
        var item = BatchedWarrantyBundle.NewBatchedWarrantyBundle("code-774", "name-656");
        item.Status = BatchedWarrantyBundleStatus.Failed;
        Check.Fails(() => item.MoveTo(BatchedWarrantyBundleStatus.New), "move");
    }

    public static void RunAll()
    {
        BatchedWarrantyBundleStartsInNew();
        BatchedWarrantyBundleMovesToQueued();
        BatchedWarrantyBundleRejectsMovesFromFailed();
    }
}
