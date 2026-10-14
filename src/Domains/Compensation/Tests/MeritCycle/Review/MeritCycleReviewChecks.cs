namespace Acme.Workforce.Compensation.Tests.MeritCycle.Review;

using Acme.Workforce.Compensation.MeritCycle.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class MeritCycleReviewChecks
{
    public static void MeritCycleReviewStartsInProposed()
    {
        var item = MeritCycleReview.NewMeritCycleReview("code-139", "title-698");
        Check.Equal(item.Status, MeritCycleReviewStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleReviewMovesToWithdrawn()
    {
        var item = MeritCycleReview.NewMeritCycleReview("code-139", "title-698");
        try
        {
            item.MoveTo(MeritCycleReviewStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleReviewStatus.Withdrawn, "status");
    }

    public static void MeritCycleReviewRejectsMovesFromWithdrawn()
    {
        var item = MeritCycleReview.NewMeritCycleReview("code-139", "title-698");
        item.Status = MeritCycleReviewStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(MeritCycleReviewStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        MeritCycleReviewStartsInProposed();
        MeritCycleReviewMovesToWithdrawn();
        MeritCycleReviewRejectsMovesFromWithdrawn();
    }
}
