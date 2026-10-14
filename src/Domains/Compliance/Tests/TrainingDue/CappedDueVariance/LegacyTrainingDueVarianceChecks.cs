namespace Acme.Workforce.Compliance.Tests.TrainingDue.CappedDueVariance;

using Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

/// <summary>Checks of capped due variance code, run by the project's runner.</summary>
public static class LegacyTrainingDueVarianceChecks
{
    public static void LegacyTrainingDueVarianceStartsInDraft()
    {
        var item = LegacyTrainingDueVariance.NewLegacyTrainingDueVariance("code-870", "label-513");
        Check.Equal(item.Status, LegacyTrainingDueVarianceStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyTrainingDueVarianceMovesToPending()
    {
        var item = LegacyTrainingDueVariance.NewLegacyTrainingDueVariance("code-870", "label-513");
        try
        {
            item.MoveTo(LegacyTrainingDueVarianceStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyTrainingDueVarianceStatus.Pending, "status");
    }

    public static void LegacyTrainingDueVarianceRejectsMovesFromClosed()
    {
        var item = LegacyTrainingDueVariance.NewLegacyTrainingDueVariance("code-870", "label-513");
        item.Status = LegacyTrainingDueVarianceStatus.Closed;
        Check.Fails(() => item.MoveTo(LegacyTrainingDueVarianceStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LegacyTrainingDueVarianceStartsInDraft();
        LegacyTrainingDueVarianceMovesToPending();
        LegacyTrainingDueVarianceRejectsMovesFromClosed();
    }
}
