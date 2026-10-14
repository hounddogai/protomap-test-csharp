namespace Acme.Workforce.Orgdesign.Tests.Position.Schedule;

using Acme.Workforce.Orgdesign.Position.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class RollingPositionAssignmentChecks
{
    public static void RollingPositionAssignmentStartsInNew()
    {
        var item = RollingPositionAssignment.NewRollingPositionAssignment("code-937", "title-325");
        Check.Equal(item.Status, RollingPositionAssignmentStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingPositionAssignmentMovesToQueued()
    {
        var item = RollingPositionAssignment.NewRollingPositionAssignment("code-937", "title-325");
        try
        {
            item.MoveTo(RollingPositionAssignmentStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingPositionAssignmentStatus.Queued, "status");
    }

    public static void RollingPositionAssignmentRejectsMovesFromFailed()
    {
        var item = RollingPositionAssignment.NewRollingPositionAssignment("code-937", "title-325");
        item.Status = RollingPositionAssignmentStatus.Failed;
        Check.Fails(() => item.MoveTo(RollingPositionAssignmentStatus.New), "move");
    }

    public static void RunAll()
    {
        RollingPositionAssignmentStartsInNew();
        RollingPositionAssignmentMovesToQueued();
        RollingPositionAssignmentRejectsMovesFromFailed();
    }
}
