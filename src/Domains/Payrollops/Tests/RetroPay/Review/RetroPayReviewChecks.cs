namespace Acme.Workforce.Payrollops.Tests.RetroPay.Review;

using Acme.Workforce.Payrollops.RetroPay.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RetroPayReviewChecks
{
    public static void RetroPayReviewStartsInCreated()
    {
        var item = RetroPayReview.NewRetroPayReview("code-562", "title-179");
        Check.Equal(item.Status, RetroPayReviewStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPayReviewMovesToRetired()
    {
        var item = RetroPayReview.NewRetroPayReview("code-562", "title-179");
        try
        {
            item.MoveTo(RetroPayReviewStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPayReviewStatus.Retired, "status");
    }

    public static void RetroPayReviewRejectsMovesFromRetired()
    {
        var item = RetroPayReview.NewRetroPayReview("code-562", "title-179");
        item.Status = RetroPayReviewStatus.Retired;
        Check.Fails(() => item.MoveTo(RetroPayReviewStatus.Created), "move");
    }

    public static void RunAll()
    {
        RetroPayReviewStartsInCreated();
        RetroPayReviewMovesToRetired();
        RetroPayReviewRejectsMovesFromRetired();
    }
}
