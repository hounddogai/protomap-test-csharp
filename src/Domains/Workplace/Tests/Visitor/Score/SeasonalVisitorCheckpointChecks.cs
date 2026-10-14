namespace Acme.Workforce.Workplace.Tests.Visitor.Score;

using Acme.Workforce.Workplace.Visitor.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class SeasonalVisitorCheckpointChecks
{
    public static void SeasonalVisitorCheckpointStartsInIdle()
    {
        var item = SeasonalVisitorCheckpoint.NewSeasonalVisitorCheckpoint("code-809", "title-56");
        Check.Equal(item.Status, SeasonalVisitorCheckpointStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalVisitorCheckpointMovesToStopped()
    {
        var item = SeasonalVisitorCheckpoint.NewSeasonalVisitorCheckpoint("code-809", "title-56");
        try
        {
            item.MoveTo(SeasonalVisitorCheckpointStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalVisitorCheckpointStatus.Stopped, "status");
    }

    public static void SeasonalVisitorCheckpointRejectsMovesFromStopped()
    {
        var item = SeasonalVisitorCheckpoint.NewSeasonalVisitorCheckpoint("code-809", "title-56");
        item.Status = SeasonalVisitorCheckpointStatus.Stopped;
        Check.Fails(() => item.MoveTo(SeasonalVisitorCheckpointStatus.Idle), "move");
    }

    public static void RunAll()
    {
        SeasonalVisitorCheckpointStartsInIdle();
        SeasonalVisitorCheckpointMovesToStopped();
        SeasonalVisitorCheckpointRejectsMovesFromStopped();
    }
}
