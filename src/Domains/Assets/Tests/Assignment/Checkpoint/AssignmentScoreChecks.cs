namespace Acme.Workforce.Assets.Tests.Assignment.Checkpoint;

using Acme.Workforce.Assets.Assignment.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AssignmentScoreChecks
{
    public static void AssignmentScoreStartsInOpen()
    {
        var item = AssignmentScore.NewAssignmentScore("code-661", "title-112");
        Check.Equal(item.Status, AssignmentScoreStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentScoreMovesToInReview()
    {
        var item = AssignmentScore.NewAssignmentScore("code-661", "title-112");
        try
        {
            item.MoveTo(AssignmentScoreStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentScoreStatus.InReview, "status");
    }

    public static void AssignmentScoreRejectsMovesFromArchived()
    {
        var item = AssignmentScore.NewAssignmentScore("code-661", "title-112");
        item.Status = AssignmentScoreStatus.Archived;
        Check.Fails(() => item.MoveTo(AssignmentScoreStatus.Open), "move");
    }

    public static void RunAll()
    {
        AssignmentScoreStartsInOpen();
        AssignmentScoreMovesToInReview();
        AssignmentScoreRejectsMovesFromArchived();
    }
}
