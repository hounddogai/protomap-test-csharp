namespace Acme.Workforce.Compliance.Tests.TrainingDue.Batch;

using Acme.Workforce.Compliance.TrainingDue.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PrimaryTrainingDueBatchChecks
{
    public static void PrimaryTrainingDueBatchStartsInCreated()
    {
        var item = PrimaryTrainingDueBatch.NewPrimaryTrainingDueBatch("code-837", "name-364");
        Check.Equal(item.Status, PrimaryTrainingDueBatchStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryTrainingDueBatchMovesToPublished()
    {
        var item = PrimaryTrainingDueBatch.NewPrimaryTrainingDueBatch("code-837", "name-364");
        try
        {
            item.MoveTo(PrimaryTrainingDueBatchStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryTrainingDueBatchStatus.Published, "status");
    }

    public static void PrimaryTrainingDueBatchRejectsMovesFromRetired()
    {
        var item = PrimaryTrainingDueBatch.NewPrimaryTrainingDueBatch("code-837", "name-364");
        item.Status = PrimaryTrainingDueBatchStatus.Retired;
        Check.Fails(() => item.MoveTo(PrimaryTrainingDueBatchStatus.Created), "move");
    }

    public static void RunAll()
    {
        PrimaryTrainingDueBatchStartsInCreated();
        PrimaryTrainingDueBatchMovesToPublished();
        PrimaryTrainingDueBatchRejectsMovesFromRetired();
    }
}
