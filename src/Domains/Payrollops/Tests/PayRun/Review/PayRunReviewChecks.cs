namespace Acme.Workforce.Payrollops.Tests.PayRun.Review;

using Acme.Workforce.Payrollops.PayRun.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class PayRunReviewChecks
{
    public static void PayRunReviewStartsInRequested()
    {
        var item = PayRunReview.NewPayRunReview("code-995", "title-392");
        Check.Equal(item.Status, PayRunReviewStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayRunReviewMovesToRevoked()
    {
        var item = PayRunReview.NewPayRunReview("code-995", "title-392");
        try
        {
            item.MoveTo(PayRunReviewStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayRunReviewStatus.Revoked, "status");
    }

    public static void PayRunReviewRejectsMovesFromExpired()
    {
        var item = PayRunReview.NewPayRunReview("code-995", "title-392");
        item.Status = PayRunReviewStatus.Expired;
        Check.Fails(() => item.MoveTo(PayRunReviewStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PayRunReviewStartsInRequested();
        PayRunReviewMovesToRevoked();
        PayRunReviewRejectsMovesFromExpired();
    }
}
