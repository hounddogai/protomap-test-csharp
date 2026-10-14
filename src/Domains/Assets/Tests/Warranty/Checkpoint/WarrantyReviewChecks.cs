namespace Acme.Workforce.Assets.Tests.Warranty.Checkpoint;

using Acme.Workforce.Assets.Warranty.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class WarrantyReviewChecks
{
    public static void WarrantyReviewStartsInIdle()
    {
        var item = WarrantyReview.NewWarrantyReview("code-888", "name-644");
        Check.Equal(item.Status, WarrantyReviewStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantyReviewMovesToWarming()
    {
        var item = WarrantyReview.NewWarrantyReview("code-888", "name-644");
        try
        {
            item.MoveTo(WarrantyReviewStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantyReviewStatus.Warming, "status");
    }

    public static void WarrantyReviewRejectsMovesFromStopped()
    {
        var item = WarrantyReview.NewWarrantyReview("code-888", "name-644");
        item.Status = WarrantyReviewStatus.Stopped;
        Check.Fails(() => item.MoveTo(WarrantyReviewStatus.Idle), "move");
    }

    public static void RunAll()
    {
        WarrantyReviewStartsInIdle();
        WarrantyReviewMovesToWarming();
        WarrantyReviewRejectsMovesFromStopped();
    }
}
