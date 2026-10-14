namespace Acme.Workforce.Workplace.Tests.Visitor.Score;

using Acme.Workforce.Workplace.Visitor.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class LenientVisitorCheckpointChecks
{
    public static void LenientVisitorCheckpointStartsInIdle()
    {
        var item = LenientVisitorCheckpoint.NewLenientVisitorCheckpoint("code-869", "name-199");
        Check.Equal(item.Status, LenientVisitorCheckpointStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientVisitorCheckpointMovesToDraining()
    {
        var item = LenientVisitorCheckpoint.NewLenientVisitorCheckpoint("code-869", "name-199");
        try
        {
            item.MoveTo(LenientVisitorCheckpointStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientVisitorCheckpointStatus.Draining, "status");
    }

    public static void LenientVisitorCheckpointRejectsMovesFromStopped()
    {
        var item = LenientVisitorCheckpoint.NewLenientVisitorCheckpoint("code-869", "name-199");
        item.Status = LenientVisitorCheckpointStatus.Stopped;
        Check.Fails(() => item.MoveTo(LenientVisitorCheckpointStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LenientVisitorCheckpointStartsInIdle();
        LenientVisitorCheckpointMovesToDraining();
        LenientVisitorCheckpointRejectsMovesFromStopped();
    }
}
