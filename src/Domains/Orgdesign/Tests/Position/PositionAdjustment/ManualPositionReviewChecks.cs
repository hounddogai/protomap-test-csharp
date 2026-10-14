namespace Acme.Workforce.Orgdesign.Tests.Position.PositionAdjustment;

using Acme.Workforce.Orgdesign.Position.PositionAdjustment;

/// <summary>Checks of position adjustment code, run by the project's runner.</summary>
public static class ManualPositionReviewChecks
{
    public static void ManualPositionReviewStartsInOpen()
    {
        var item = ManualPositionReview.NewManualPositionReview("code-669", "name-271");
        Check.Equal(item.Status, ManualPositionReviewStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualPositionReviewMovesToResolved()
    {
        var item = ManualPositionReview.NewManualPositionReview("code-669", "name-271");
        try
        {
            item.MoveTo(ManualPositionReviewStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualPositionReviewStatus.Resolved, "status");
    }

    public static void ManualPositionReviewRejectsMovesFromArchived()
    {
        var item = ManualPositionReview.NewManualPositionReview("code-669", "name-271");
        item.Status = ManualPositionReviewStatus.Archived;
        Check.Fails(() => item.MoveTo(ManualPositionReviewStatus.Open), "move");
    }

    public static void RunAll()
    {
        ManualPositionReviewStartsInOpen();
        ManualPositionReviewMovesToResolved();
        ManualPositionReviewRejectsMovesFromArchived();
    }
}
