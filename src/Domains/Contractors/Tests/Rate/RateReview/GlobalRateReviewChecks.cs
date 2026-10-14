namespace Acme.Workforce.Contractors.Tests.Rate.RateReview;

using Acme.Workforce.Contractors.Rate.RateReview;

/// <summary>Checks of rate review code, run by the project's runner.</summary>
public static class GlobalRateReviewChecks
{
    public static void GlobalRateReviewStartsInRequested()
    {
        var item = GlobalRateReview.NewGlobalRateReview("code-216", "label-528");
        Check.Equal(item.Status, GlobalRateReviewStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalRateReviewMovesToGranted()
    {
        var item = GlobalRateReview.NewGlobalRateReview("code-216", "label-528");
        try
        {
            item.MoveTo(GlobalRateReviewStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalRateReviewStatus.Granted, "status");
    }

    public static void GlobalRateReviewRejectsMovesFromExpired()
    {
        var item = GlobalRateReview.NewGlobalRateReview("code-216", "label-528");
        item.Status = GlobalRateReviewStatus.Expired;
        Check.Fails(() => item.MoveTo(GlobalRateReviewStatus.Requested), "move");
    }

    public static void RunAll()
    {
        GlobalRateReviewStartsInRequested();
        GlobalRateReviewMovesToGranted();
        GlobalRateReviewRejectsMovesFromExpired();
    }
}
