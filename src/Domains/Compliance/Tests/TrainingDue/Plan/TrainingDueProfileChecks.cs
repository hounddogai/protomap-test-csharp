namespace Acme.Workforce.Compliance.Tests.TrainingDue.Plan;

using Acme.Workforce.Compliance.TrainingDue.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class TrainingDueProfileChecks
{
    public static void TrainingDueProfileStartsInDraft()
    {
        var item = TrainingDueProfile.NewTrainingDueProfile("code-900", "label-500");
        Check.Equal(item.Status, TrainingDueProfileStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDueProfileMovesToActive()
    {
        var item = TrainingDueProfile.NewTrainingDueProfile("code-900", "label-500");
        try
        {
            item.MoveTo(TrainingDueProfileStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDueProfileStatus.Active, "status");
    }

    public static void TrainingDueProfileRejectsMovesFromClosed()
    {
        var item = TrainingDueProfile.NewTrainingDueProfile("code-900", "label-500");
        item.Status = TrainingDueProfileStatus.Closed;
        Check.Fails(() => item.MoveTo(TrainingDueProfileStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TrainingDueProfileStartsInDraft();
        TrainingDueProfileMovesToActive();
        TrainingDueProfileRejectsMovesFromClosed();
    }
}
