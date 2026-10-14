namespace Acme.Workforce.Compliance.Tests.TrainingDue.Budget;

using Acme.Workforce.Compliance.TrainingDue.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class LegacyTrainingDueBatchChecks
{
    public static void LegacyTrainingDueBatchStartsInOpen()
    {
        var item = LegacyTrainingDueBatch.NewLegacyTrainingDueBatch("code-872", "name-92");
        Check.Equal(item.Status, LegacyTrainingDueBatchStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyTrainingDueBatchMovesToArchived()
    {
        var item = LegacyTrainingDueBatch.NewLegacyTrainingDueBatch("code-872", "name-92");
        try
        {
            item.MoveTo(LegacyTrainingDueBatchStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyTrainingDueBatchStatus.Archived, "status");
    }

    public static void LegacyTrainingDueBatchRejectsMovesFromArchived()
    {
        var item = LegacyTrainingDueBatch.NewLegacyTrainingDueBatch("code-872", "name-92");
        item.Status = LegacyTrainingDueBatchStatus.Archived;
        Check.Fails(() => item.MoveTo(LegacyTrainingDueBatchStatus.Open), "move");
    }

    public static void RunAll()
    {
        LegacyTrainingDueBatchStartsInOpen();
        LegacyTrainingDueBatchMovesToArchived();
        LegacyTrainingDueBatchRejectsMovesFromArchived();
    }
}
