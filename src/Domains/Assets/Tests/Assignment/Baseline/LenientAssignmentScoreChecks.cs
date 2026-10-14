namespace Acme.Workforce.Assets.Tests.Assignment.Baseline;

using Acme.Workforce.Assets.Assignment.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class LenientAssignmentScoreChecks
{
    public static void LenientAssignmentScoreStartsInProposed()
    {
        var item = LenientAssignmentScore.NewLenientAssignmentScore("code-227", "label-471");
        Check.Equal(item.Status, LenientAssignmentScoreStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientAssignmentScoreMovesToApproved()
    {
        var item = LenientAssignmentScore.NewLenientAssignmentScore("code-227", "label-471");
        try
        {
            item.MoveTo(LenientAssignmentScoreStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientAssignmentScoreStatus.Approved, "status");
    }

    public static void LenientAssignmentScoreRejectsMovesFromWithdrawn()
    {
        var item = LenientAssignmentScore.NewLenientAssignmentScore("code-227", "label-471");
        item.Status = LenientAssignmentScoreStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LenientAssignmentScoreStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LenientAssignmentScoreStartsInProposed();
        LenientAssignmentScoreMovesToApproved();
        LenientAssignmentScoreRejectsMovesFromWithdrawn();
    }
}
