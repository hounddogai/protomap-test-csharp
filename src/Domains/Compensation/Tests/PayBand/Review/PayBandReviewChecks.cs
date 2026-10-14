namespace Acme.Workforce.Compensation.Tests.PayBand.Review;

using Acme.Workforce.Compensation.PayBand.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class PayBandReviewChecks
{
    public static void PayBandReviewStartsInOpen()
    {
        var item = PayBandReview.NewPayBandReview("code-647", "name-275");
        Check.Equal(item.Status, PayBandReviewStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayBandReviewMovesToReopened()
    {
        var item = PayBandReview.NewPayBandReview("code-647", "name-275");
        try
        {
            item.MoveTo(PayBandReviewStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayBandReviewStatus.Reopened, "status");
    }

    public static void PayBandReviewRejectsMovesFromArchived()
    {
        var item = PayBandReview.NewPayBandReview("code-647", "name-275");
        item.Status = PayBandReviewStatus.Archived;
        Check.Fails(() => item.MoveTo(PayBandReviewStatus.Open), "move");
    }

    public static void RunAll()
    {
        PayBandReviewStartsInOpen();
        PayBandReviewMovesToReopened();
        PayBandReviewRejectsMovesFromArchived();
    }
}
