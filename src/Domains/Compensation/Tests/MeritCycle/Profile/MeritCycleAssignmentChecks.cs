namespace Acme.Workforce.Compensation.Tests.MeritCycle.Profile;

using Acme.Workforce.Compensation.MeritCycle.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class MeritCycleAssignmentChecks
{
    public static void MeritCycleAssignmentStartsInIdle()
    {
        var item = MeritCycleAssignment.NewMeritCycleAssignment("code-508", "name-329");
        Check.Equal(item.Status, MeritCycleAssignmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleAssignmentMovesToDraining()
    {
        var item = MeritCycleAssignment.NewMeritCycleAssignment("code-508", "name-329");
        try
        {
            item.MoveTo(MeritCycleAssignmentStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleAssignmentStatus.Draining, "status");
    }

    public static void MeritCycleAssignmentRejectsMovesFromStopped()
    {
        var item = MeritCycleAssignment.NewMeritCycleAssignment("code-508", "name-329");
        item.Status = MeritCycleAssignmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(MeritCycleAssignmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        MeritCycleAssignmentStartsInIdle();
        MeritCycleAssignmentMovesToDraining();
        MeritCycleAssignmentRejectsMovesFromStopped();
    }
}
