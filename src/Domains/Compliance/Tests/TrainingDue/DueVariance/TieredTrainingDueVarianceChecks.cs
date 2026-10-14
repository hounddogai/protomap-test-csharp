namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueVariance;

using Acme.Workforce.Compliance.TrainingDue.DueVariance;

/// <summary>Checks of due variance code, run by the project's runner.</summary>
public static class TieredTrainingDueVarianceChecks
{
    public static void TieredTrainingDueVarianceStartsInDraft()
    {
        var item = TieredTrainingDueVariance.NewTieredTrainingDueVariance("code-62", "title-708");
        Check.Equal(item.Status, TieredTrainingDueVarianceStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredTrainingDueVarianceMovesToPending()
    {
        var item = TieredTrainingDueVariance.NewTieredTrainingDueVariance("code-62", "title-708");
        try
        {
            item.MoveTo(TieredTrainingDueVarianceStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredTrainingDueVarianceStatus.Pending, "status");
    }

    public static void TieredTrainingDueVarianceRejectsMovesFromClosed()
    {
        var item = TieredTrainingDueVariance.NewTieredTrainingDueVariance("code-62", "title-708");
        item.Status = TieredTrainingDueVarianceStatus.Closed;
        Check.Fails(() => item.MoveTo(TieredTrainingDueVarianceStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TieredTrainingDueVarianceStartsInDraft();
        TieredTrainingDueVarianceMovesToPending();
        TieredTrainingDueVarianceRejectsMovesFromClosed();
    }
}
