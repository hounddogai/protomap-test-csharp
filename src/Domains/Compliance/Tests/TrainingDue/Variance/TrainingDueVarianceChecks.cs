namespace Acme.Workforce.Compliance.Tests.TrainingDue.Variance;

using Acme.Workforce.Compliance.TrainingDue.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class TrainingDueVarianceChecks
{
    public static void TrainingDueVarianceStartsInNew()
    {
        var item = TrainingDueVariance.NewTrainingDueVariance("code-873", "name-160");
        Check.Equal(item.Status, TrainingDueVarianceStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDueVarianceMovesToDone()
    {
        var item = TrainingDueVariance.NewTrainingDueVariance("code-873", "name-160");
        try
        {
            item.MoveTo(TrainingDueVarianceStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDueVarianceStatus.Done, "status");
    }

    public static void TrainingDueVarianceRejectsMovesFromFailed()
    {
        var item = TrainingDueVariance.NewTrainingDueVariance("code-873", "name-160");
        item.Status = TrainingDueVarianceStatus.Failed;
        Check.Fails(() => item.MoveTo(TrainingDueVarianceStatus.New), "move");
    }

    public static void RunAll()
    {
        TrainingDueVarianceStartsInNew();
        TrainingDueVarianceMovesToDone();
        TrainingDueVarianceRejectsMovesFromFailed();
    }
}
