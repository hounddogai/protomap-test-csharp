namespace Acme.Workforce.Compensation.Tests.MeritCycle.Review;

using Acme.Workforce.Compensation.MeritCycle.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class LenientMeritCycleReviewChecks
{
    public static void LenientMeritCycleReviewStartsInNew()
    {
        var item = LenientMeritCycleReview.NewLenientMeritCycleReview("code-408", "title-730");
        Check.Equal(item.Status, LenientMeritCycleReviewStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientMeritCycleReviewMovesToRunning()
    {
        var item = LenientMeritCycleReview.NewLenientMeritCycleReview("code-408", "title-730");
        try
        {
            item.MoveTo(LenientMeritCycleReviewStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientMeritCycleReviewStatus.Running, "status");
    }

    public static void LenientMeritCycleReviewRejectsMovesFromFailed()
    {
        var item = LenientMeritCycleReview.NewLenientMeritCycleReview("code-408", "title-730");
        item.Status = LenientMeritCycleReviewStatus.Failed;
        Check.Fails(() => item.MoveTo(LenientMeritCycleReviewStatus.New), "move");
    }

    public static void RunAll()
    {
        LenientMeritCycleReviewStartsInNew();
        LenientMeritCycleReviewMovesToRunning();
        LenientMeritCycleReviewRejectsMovesFromFailed();
    }
}
