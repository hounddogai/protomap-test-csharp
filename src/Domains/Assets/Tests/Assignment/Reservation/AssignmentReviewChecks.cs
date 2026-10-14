namespace Acme.Workforce.Assets.Tests.Assignment.Reservation;

using Acme.Workforce.Assets.Assignment.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class AssignmentReviewChecks
{
    public static void AssignmentReviewStartsInNew()
    {
        var item = AssignmentReview.NewAssignmentReview("code-67", "title-385");
        Check.Equal(item.Status, AssignmentReviewStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentReviewMovesToFailed()
    {
        var item = AssignmentReview.NewAssignmentReview("code-67", "title-385");
        try
        {
            item.MoveTo(AssignmentReviewStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentReviewStatus.Failed, "status");
    }

    public static void AssignmentReviewRejectsMovesFromFailed()
    {
        var item = AssignmentReview.NewAssignmentReview("code-67", "title-385");
        item.Status = AssignmentReviewStatus.Failed;
        Check.Fails(() => item.MoveTo(AssignmentReviewStatus.New), "move");
    }

    public static void RunAll()
    {
        AssignmentReviewStartsInNew();
        AssignmentReviewMovesToFailed();
        AssignmentReviewRejectsMovesFromFailed();
    }
}
