namespace Acme.Workforce.Orgdesign.Tests.Position.Plan;

using Acme.Workforce.Orgdesign.Position.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class PositionReviewChecks
{
    public static void PositionReviewStartsInOpen()
    {
        var item = PositionReview.NewPositionReview("code-634", "name-584");
        Check.Equal(item.Status, PositionReviewStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PositionReviewMovesToResolved()
    {
        var item = PositionReview.NewPositionReview("code-634", "name-584");
        try
        {
            item.MoveTo(PositionReviewStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PositionReviewStatus.Resolved, "status");
    }

    public static void PositionReviewRejectsMovesFromArchived()
    {
        var item = PositionReview.NewPositionReview("code-634", "name-584");
        item.Status = PositionReviewStatus.Archived;
        Check.Fails(() => item.MoveTo(PositionReviewStatus.Open), "move");
    }

    public static void RunAll()
    {
        PositionReviewStartsInOpen();
        PositionReviewMovesToResolved();
        PositionReviewRejectsMovesFromArchived();
    }
}
