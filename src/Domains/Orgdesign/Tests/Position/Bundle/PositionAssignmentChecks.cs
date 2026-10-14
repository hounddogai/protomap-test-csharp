namespace Acme.Workforce.Orgdesign.Tests.Position.Bundle;

using Acme.Workforce.Orgdesign.Position.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PositionAssignmentChecks
{
    public static void PositionAssignmentStartsInNew()
    {
        var item = PositionAssignment.NewPositionAssignment("code-4", "title-987");
        Check.Equal(item.Status, PositionAssignmentStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PositionAssignmentMovesToRunning()
    {
        var item = PositionAssignment.NewPositionAssignment("code-4", "title-987");
        try
        {
            item.MoveTo(PositionAssignmentStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PositionAssignmentStatus.Running, "status");
    }

    public static void PositionAssignmentRejectsMovesFromFailed()
    {
        var item = PositionAssignment.NewPositionAssignment("code-4", "title-987");
        item.Status = PositionAssignmentStatus.Failed;
        Check.Fails(() => item.MoveTo(PositionAssignmentStatus.New), "move");
    }

    public static void RunAll()
    {
        PositionAssignmentStartsInNew();
        PositionAssignmentMovesToRunning();
        PositionAssignmentRejectsMovesFromFailed();
    }
}
