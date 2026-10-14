namespace Acme.Workforce.Compliance.Tests.TrainingDue.Window;

using Acme.Workforce.Compliance.TrainingDue.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TrainingDueCycleChecks
{
    public static void TrainingDueCycleStartsInPlanned()
    {
        var item = TrainingDueCycle.NewTrainingDueCycle("code-554", "title-903");
        Check.Equal(item.Status, TrainingDueCycleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDueCycleMovesToCompleted()
    {
        var item = TrainingDueCycle.NewTrainingDueCycle("code-554", "title-903");
        try
        {
            item.MoveTo(TrainingDueCycleStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDueCycleStatus.Completed, "status");
    }

    public static void TrainingDueCycleRejectsMovesFromCancelled()
    {
        var item = TrainingDueCycle.NewTrainingDueCycle("code-554", "title-903");
        item.Status = TrainingDueCycleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TrainingDueCycleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TrainingDueCycleStartsInPlanned();
        TrainingDueCycleMovesToCompleted();
        TrainingDueCycleRejectsMovesFromCancelled();
    }
}
