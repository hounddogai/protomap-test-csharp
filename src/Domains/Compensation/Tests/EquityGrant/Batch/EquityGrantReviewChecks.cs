namespace Acme.Workforce.Compensation.Tests.EquityGrant.Batch;

using Acme.Workforce.Compensation.EquityGrant.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class EquityGrantReviewChecks
{
    public static void EquityGrantReviewStartsInPlanned()
    {
        var item = EquityGrantReview.NewEquityGrantReview("code-48", "name-281");
        Check.Equal(item.Status, EquityGrantReviewStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EquityGrantReviewMovesToInProgress()
    {
        var item = EquityGrantReview.NewEquityGrantReview("code-48", "name-281");
        try
        {
            item.MoveTo(EquityGrantReviewStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EquityGrantReviewStatus.InProgress, "status");
    }

    public static void EquityGrantReviewRejectsMovesFromCancelled()
    {
        var item = EquityGrantReview.NewEquityGrantReview("code-48", "name-281");
        item.Status = EquityGrantReviewStatus.Cancelled;
        Check.Fails(() => item.MoveTo(EquityGrantReviewStatus.Planned), "move");
    }

    public static void RunAll()
    {
        EquityGrantReviewStartsInPlanned();
        EquityGrantReviewMovesToInProgress();
        EquityGrantReviewRejectsMovesFromCancelled();
    }
}
