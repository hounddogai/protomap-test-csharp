namespace Acme.Workforce.Payrollops.Tests.RetroPay.PaySummary;

using Acme.Workforce.Payrollops.RetroPay.PaySummary;

/// <summary>Checks of pay summary code, run by the project's runner.</summary>
public static class PendingRetroPayReviewChecks
{
    public static void PendingRetroPayReviewStartsInIdle()
    {
        var item = PendingRetroPayReview.NewPendingRetroPayReview("code-46", "name-624");
        Check.Equal(item.Status, PendingRetroPayReviewStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingRetroPayReviewMovesToDraining()
    {
        var item = PendingRetroPayReview.NewPendingRetroPayReview("code-46", "name-624");
        try
        {
            item.MoveTo(PendingRetroPayReviewStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingRetroPayReviewStatus.Draining, "status");
    }

    public static void PendingRetroPayReviewRejectsMovesFromStopped()
    {
        var item = PendingRetroPayReview.NewPendingRetroPayReview("code-46", "name-624");
        item.Status = PendingRetroPayReviewStatus.Stopped;
        Check.Fails(() => item.MoveTo(PendingRetroPayReviewStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PendingRetroPayReviewStartsInIdle();
        PendingRetroPayReviewMovesToDraining();
        PendingRetroPayReviewRejectsMovesFromStopped();
    }
}
