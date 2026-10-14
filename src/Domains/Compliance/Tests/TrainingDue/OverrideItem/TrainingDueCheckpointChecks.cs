namespace Acme.Workforce.Compliance.Tests.TrainingDue.OverrideItem;

using Acme.Workforce.Compliance.TrainingDue.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class TrainingDueCheckpointChecks
{
    public static void TrainingDueCheckpointStartsInIdle()
    {
        var item = TrainingDueCheckpoint.NewTrainingDueCheckpoint("code-863", "title-489");
        Check.Equal(item.Status, TrainingDueCheckpointStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDueCheckpointMovesToReady()
    {
        var item = TrainingDueCheckpoint.NewTrainingDueCheckpoint("code-863", "title-489");
        try
        {
            item.MoveTo(TrainingDueCheckpointStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDueCheckpointStatus.Ready, "status");
    }

    public static void TrainingDueCheckpointRejectsMovesFromStopped()
    {
        var item = TrainingDueCheckpoint.NewTrainingDueCheckpoint("code-863", "title-489");
        item.Status = TrainingDueCheckpointStatus.Stopped;
        Check.Fails(() => item.MoveTo(TrainingDueCheckpointStatus.Idle), "move");
    }

    public static void RunAll()
    {
        TrainingDueCheckpointStartsInIdle();
        TrainingDueCheckpointMovesToReady();
        TrainingDueCheckpointRejectsMovesFromStopped();
    }
}
