namespace Acme.Workforce.Orgdesign.Tests.Position.PositionBundle;

using Acme.Workforce.Orgdesign.Position.PositionBundle;

/// <summary>Checks of position bundle code, run by the project's runner.</summary>
public static class CappedPositionAssignmentChecks
{
    public static void CappedPositionAssignmentStartsInOpen()
    {
        var item = CappedPositionAssignment.NewCappedPositionAssignment("code-379", "name-69");
        Check.Equal(item.Status, CappedPositionAssignmentStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedPositionAssignmentMovesToInReview()
    {
        var item = CappedPositionAssignment.NewCappedPositionAssignment("code-379", "name-69");
        try
        {
            item.MoveTo(CappedPositionAssignmentStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedPositionAssignmentStatus.InReview, "status");
    }

    public static void CappedPositionAssignmentRejectsMovesFromArchived()
    {
        var item = CappedPositionAssignment.NewCappedPositionAssignment("code-379", "name-69");
        item.Status = CappedPositionAssignmentStatus.Archived;
        Check.Fails(() => item.MoveTo(CappedPositionAssignmentStatus.Open), "move");
    }

    public static void RunAll()
    {
        CappedPositionAssignmentStartsInOpen();
        CappedPositionAssignmentMovesToInReview();
        CappedPositionAssignmentRejectsMovesFromArchived();
    }
}
