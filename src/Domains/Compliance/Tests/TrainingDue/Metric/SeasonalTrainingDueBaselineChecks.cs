namespace Acme.Workforce.Compliance.Tests.TrainingDue.Metric;

using Acme.Workforce.Compliance.TrainingDue.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class SeasonalTrainingDueBaselineChecks
{
    public static void SeasonalTrainingDueBaselineStartsInDraft()
    {
        var item = SeasonalTrainingDueBaseline.NewSeasonalTrainingDueBaseline("code-28", "name-148");
        Check.Equal(item.Status, SeasonalTrainingDueBaselineStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalTrainingDueBaselineMovesToActive()
    {
        var item = SeasonalTrainingDueBaseline.NewSeasonalTrainingDueBaseline("code-28", "name-148");
        try
        {
            item.MoveTo(SeasonalTrainingDueBaselineStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalTrainingDueBaselineStatus.Active, "status");
    }

    public static void SeasonalTrainingDueBaselineRejectsMovesFromClosed()
    {
        var item = SeasonalTrainingDueBaseline.NewSeasonalTrainingDueBaseline("code-28", "name-148");
        item.Status = SeasonalTrainingDueBaselineStatus.Closed;
        Check.Fails(() => item.MoveTo(SeasonalTrainingDueBaselineStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SeasonalTrainingDueBaselineStartsInDraft();
        SeasonalTrainingDueBaselineMovesToActive();
        SeasonalTrainingDueBaselineRejectsMovesFromClosed();
    }
}
