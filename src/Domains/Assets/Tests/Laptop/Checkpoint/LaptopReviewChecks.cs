namespace Acme.Workforce.Assets.Tests.Laptop.Checkpoint;

using Acme.Workforce.Assets.Laptop.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class LaptopReviewChecks
{
    public static void LaptopReviewStartsInDraft()
    {
        var item = LaptopReview.NewLaptopReview("code-591", "title-743");
        Check.Equal(item.Status, LaptopReviewStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopReviewMovesToPending()
    {
        var item = LaptopReview.NewLaptopReview("code-591", "title-743");
        try
        {
            item.MoveTo(LaptopReviewStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopReviewStatus.Pending, "status");
    }

    public static void LaptopReviewRejectsMovesFromClosed()
    {
        var item = LaptopReview.NewLaptopReview("code-591", "title-743");
        item.Status = LaptopReviewStatus.Closed;
        Check.Fails(() => item.MoveTo(LaptopReviewStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LaptopReviewStartsInDraft();
        LaptopReviewMovesToPending();
        LaptopReviewRejectsMovesFromClosed();
    }
}
