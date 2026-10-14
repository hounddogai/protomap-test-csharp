namespace Acme.Workforce.Assets.Tests.Assignment.Checkpoint;

using Acme.Workforce.Assets.Assignment.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class StrictAssignmentScoreChecks
{
    public static void StrictAssignmentScoreStartsInNew()
    {
        var item = StrictAssignmentScore.NewStrictAssignmentScore("code-461", "name-530");
        Check.Equal(item.Status, StrictAssignmentScoreStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictAssignmentScoreMovesToQueued()
    {
        var item = StrictAssignmentScore.NewStrictAssignmentScore("code-461", "name-530");
        try
        {
            item.MoveTo(StrictAssignmentScoreStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictAssignmentScoreStatus.Queued, "status");
    }

    public static void StrictAssignmentScoreRejectsMovesFromFailed()
    {
        var item = StrictAssignmentScore.NewStrictAssignmentScore("code-461", "name-530");
        item.Status = StrictAssignmentScoreStatus.Failed;
        Check.Fails(() => item.MoveTo(StrictAssignmentScoreStatus.New), "move");
    }

    public static void RunAll()
    {
        StrictAssignmentScoreStartsInNew();
        StrictAssignmentScoreMovesToQueued();
        StrictAssignmentScoreRejectsMovesFromFailed();
    }
}
