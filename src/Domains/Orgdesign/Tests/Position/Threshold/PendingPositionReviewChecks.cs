namespace Acme.Workforce.Orgdesign.Tests.Position.Threshold;

using Acme.Workforce.Orgdesign.Position.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PendingPositionReviewChecks
{
    public static void PendingPositionReviewStartsInCreated()
    {
        var item = PendingPositionReview.NewPendingPositionReview("code-211", "name-282");
        Check.Equal(item.Status, PendingPositionReviewStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingPositionReviewMovesToPublished()
    {
        var item = PendingPositionReview.NewPendingPositionReview("code-211", "name-282");
        try
        {
            item.MoveTo(PendingPositionReviewStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingPositionReviewStatus.Published, "status");
    }

    public static void PendingPositionReviewRejectsMovesFromRetired()
    {
        var item = PendingPositionReview.NewPendingPositionReview("code-211", "name-282");
        item.Status = PendingPositionReviewStatus.Retired;
        Check.Fails(() => item.MoveTo(PendingPositionReviewStatus.Created), "move");
    }

    public static void RunAll()
    {
        PendingPositionReviewStartsInCreated();
        PendingPositionReviewMovesToPublished();
        PendingPositionReviewRejectsMovesFromRetired();
    }
}
