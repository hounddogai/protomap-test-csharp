namespace Acme.Workforce.Workplace.Tests.Visitor.Review;

using Acme.Workforce.Workplace.Visitor.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class VisitorReviewChecks
{
    public static void VisitorReviewStartsInPlanned()
    {
        var item = VisitorReview.NewVisitorReview("code-35", "name-138");
        Check.Equal(item.Status, VisitorReviewStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorReviewMovesToInProgress()
    {
        var item = VisitorReview.NewVisitorReview("code-35", "name-138");
        try
        {
            item.MoveTo(VisitorReviewStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorReviewStatus.InProgress, "status");
    }

    public static void VisitorReviewRejectsMovesFromCancelled()
    {
        var item = VisitorReview.NewVisitorReview("code-35", "name-138");
        item.Status = VisitorReviewStatus.Cancelled;
        Check.Fails(() => item.MoveTo(VisitorReviewStatus.Planned), "move");
    }

    public static void RunAll()
    {
        VisitorReviewStartsInPlanned();
        VisitorReviewMovesToInProgress();
        VisitorReviewRejectsMovesFromCancelled();
    }
}
