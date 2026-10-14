namespace Acme.Workforce.Compliance.Tests.TrainingDue.Schedule;

using Acme.Workforce.Compliance.TrainingDue.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class TrainingDueBundleChecks
{
    public static void TrainingDueBundleStartsInIdle()
    {
        var item = TrainingDueBundle.NewTrainingDueBundle("code-975", "title-831");
        Check.Equal(item.Status, TrainingDueBundleStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDueBundleMovesToReady()
    {
        var item = TrainingDueBundle.NewTrainingDueBundle("code-975", "title-831");
        try
        {
            item.MoveTo(TrainingDueBundleStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDueBundleStatus.Ready, "status");
    }

    public static void TrainingDueBundleRejectsMovesFromStopped()
    {
        var item = TrainingDueBundle.NewTrainingDueBundle("code-975", "title-831");
        item.Status = TrainingDueBundleStatus.Stopped;
        Check.Fails(() => item.MoveTo(TrainingDueBundleStatus.Idle), "move");
    }

    public static void RunAll()
    {
        TrainingDueBundleStartsInIdle();
        TrainingDueBundleMovesToReady();
        TrainingDueBundleRejectsMovesFromStopped();
    }
}
