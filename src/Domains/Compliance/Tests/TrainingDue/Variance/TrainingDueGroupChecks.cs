namespace Acme.Workforce.Compliance.Tests.TrainingDue.Variance;

using Acme.Workforce.Compliance.TrainingDue.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class TrainingDueGroupChecks
{
    public static void TrainingDueGroupStartsInIdle()
    {
        var item = TrainingDueGroup.NewTrainingDueGroup("code-784", "title-37");
        Check.Equal(item.Status, TrainingDueGroupStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDueGroupMovesToWarming()
    {
        var item = TrainingDueGroup.NewTrainingDueGroup("code-784", "title-37");
        try
        {
            item.MoveTo(TrainingDueGroupStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDueGroupStatus.Warming, "status");
    }

    public static void TrainingDueGroupRejectsMovesFromStopped()
    {
        var item = TrainingDueGroup.NewTrainingDueGroup("code-784", "title-37");
        item.Status = TrainingDueGroupStatus.Stopped;
        Check.Fails(() => item.MoveTo(TrainingDueGroupStatus.Idle), "move");
    }

    public static void RunAll()
    {
        TrainingDueGroupStartsInIdle();
        TrainingDueGroupMovesToWarming();
        TrainingDueGroupRejectsMovesFromStopped();
    }
}
