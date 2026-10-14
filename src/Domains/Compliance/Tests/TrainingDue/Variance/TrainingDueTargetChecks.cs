namespace Acme.Workforce.Compliance.Tests.TrainingDue.Variance;

using Acme.Workforce.Compliance.TrainingDue.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class TrainingDueTargetChecks
{
    public static void TrainingDueTargetStartsInIdle()
    {
        var item = TrainingDueTarget.NewTrainingDueTarget("code-215", "name-790");
        Check.Equal(item.Status, TrainingDueTargetStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDueTargetMovesToStopped()
    {
        var item = TrainingDueTarget.NewTrainingDueTarget("code-215", "name-790");
        try
        {
            item.MoveTo(TrainingDueTargetStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDueTargetStatus.Stopped, "status");
    }

    public static void TrainingDueTargetRejectsMovesFromStopped()
    {
        var item = TrainingDueTarget.NewTrainingDueTarget("code-215", "name-790");
        item.Status = TrainingDueTargetStatus.Stopped;
        Check.Fails(() => item.MoveTo(TrainingDueTargetStatus.Idle), "move");
    }

    public static void RunAll()
    {
        TrainingDueTargetStartsInIdle();
        TrainingDueTargetMovesToStopped();
        TrainingDueTargetRejectsMovesFromStopped();
    }
}
