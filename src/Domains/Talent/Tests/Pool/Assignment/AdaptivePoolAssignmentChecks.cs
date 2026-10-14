namespace Acme.Workforce.Talent.Tests.Pool.Assignment;

using Acme.Workforce.Talent.Pool.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class AdaptivePoolAssignmentChecks
{
    public static void AdaptivePoolAssignmentStartsInIdle()
    {
        var item = AdaptivePoolAssignment.NewAdaptivePoolAssignment("code-311", "label-333");
        Check.Equal(item.Status, AdaptivePoolAssignmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptivePoolAssignmentMovesToReady()
    {
        var item = AdaptivePoolAssignment.NewAdaptivePoolAssignment("code-311", "label-333");
        try
        {
            item.MoveTo(AdaptivePoolAssignmentStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptivePoolAssignmentStatus.Ready, "status");
    }

    public static void AdaptivePoolAssignmentRejectsMovesFromStopped()
    {
        var item = AdaptivePoolAssignment.NewAdaptivePoolAssignment("code-311", "label-333");
        item.Status = AdaptivePoolAssignmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(AdaptivePoolAssignmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AdaptivePoolAssignmentStartsInIdle();
        AdaptivePoolAssignmentMovesToReady();
        AdaptivePoolAssignmentRejectsMovesFromStopped();
    }
}
