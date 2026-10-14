namespace Acme.Workforce.Compliance.Tests.TrainingDue.Assignment;

using Acme.Workforce.Compliance.TrainingDue.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class TrainingDueRollupChecks
{
    public static void TrainingDueRollupStartsInNew()
    {
        var item = TrainingDueRollup.NewTrainingDueRollup("code-985", "label-559");
        Check.Equal(item.Status, TrainingDueRollupStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDueRollupMovesToQueued()
    {
        var item = TrainingDueRollup.NewTrainingDueRollup("code-985", "label-559");
        try
        {
            item.MoveTo(TrainingDueRollupStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDueRollupStatus.Queued, "status");
    }

    public static void TrainingDueRollupRejectsMovesFromFailed()
    {
        var item = TrainingDueRollup.NewTrainingDueRollup("code-985", "label-559");
        item.Status = TrainingDueRollupStatus.Failed;
        Check.Fails(() => item.MoveTo(TrainingDueRollupStatus.New), "move");
    }

    public static void RunAll()
    {
        TrainingDueRollupStartsInNew();
        TrainingDueRollupMovesToQueued();
        TrainingDueRollupRejectsMovesFromFailed();
    }
}
