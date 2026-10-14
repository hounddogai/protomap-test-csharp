namespace Acme.Workforce.Compliance.Tests.TrainingDue.Batch;

using Acme.Workforce.Compliance.TrainingDue.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class WeightedTrainingDueProfileChecks
{
    public static void WeightedTrainingDueProfileStartsInDraft()
    {
        var item = WeightedTrainingDueProfile.NewWeightedTrainingDueProfile("code-83", "title-448");
        Check.Equal(item.Status, WeightedTrainingDueProfileStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedTrainingDueProfileMovesToSuspended()
    {
        var item = WeightedTrainingDueProfile.NewWeightedTrainingDueProfile("code-83", "title-448");
        try
        {
            item.MoveTo(WeightedTrainingDueProfileStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedTrainingDueProfileStatus.Suspended, "status");
    }

    public static void WeightedTrainingDueProfileRejectsMovesFromClosed()
    {
        var item = WeightedTrainingDueProfile.NewWeightedTrainingDueProfile("code-83", "title-448");
        item.Status = WeightedTrainingDueProfileStatus.Closed;
        Check.Fails(() => item.MoveTo(WeightedTrainingDueProfileStatus.Draft), "move");
    }

    public static void RunAll()
    {
        WeightedTrainingDueProfileStartsInDraft();
        WeightedTrainingDueProfileMovesToSuspended();
        WeightedTrainingDueProfileRejectsMovesFromClosed();
    }
}
