namespace Acme.Workforce.Compliance.Tests.TrainingDue.Mapping;

using Acme.Workforce.Compliance.TrainingDue.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class TrainingDueBatchChecks
{
    public static void TrainingDueBatchStartsInRequested()
    {
        var item = TrainingDueBatch.NewTrainingDueBatch("code-483", "name-286");
        Check.Equal(item.Status, TrainingDueBatchStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDueBatchMovesToGranted()
    {
        var item = TrainingDueBatch.NewTrainingDueBatch("code-483", "name-286");
        try
        {
            item.MoveTo(TrainingDueBatchStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDueBatchStatus.Granted, "status");
    }

    public static void TrainingDueBatchRejectsMovesFromExpired()
    {
        var item = TrainingDueBatch.NewTrainingDueBatch("code-483", "name-286");
        item.Status = TrainingDueBatchStatus.Expired;
        Check.Fails(() => item.MoveTo(TrainingDueBatchStatus.Requested), "move");
    }

    public static void RunAll()
    {
        TrainingDueBatchStartsInRequested();
        TrainingDueBatchMovesToGranted();
        TrainingDueBatchRejectsMovesFromExpired();
    }
}
