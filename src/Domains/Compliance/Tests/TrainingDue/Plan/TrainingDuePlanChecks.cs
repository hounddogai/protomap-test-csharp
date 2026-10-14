namespace Acme.Workforce.Compliance.Tests.TrainingDue.Plan;

using Acme.Workforce.Compliance.TrainingDue.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class TrainingDuePlanChecks
{
    public static void TrainingDuePlanStartsInDraft()
    {
        var item = TrainingDuePlan.NewTrainingDuePlan("code-211", "name-913");
        Check.Equal(item.Status, TrainingDuePlanStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDuePlanMovesToActive()
    {
        var item = TrainingDuePlan.NewTrainingDuePlan("code-211", "name-913");
        try
        {
            item.MoveTo(TrainingDuePlanStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDuePlanStatus.Active, "status");
    }

    public static void TrainingDuePlanRejectsMovesFromClosed()
    {
        var item = TrainingDuePlan.NewTrainingDuePlan("code-211", "name-913");
        item.Status = TrainingDuePlanStatus.Closed;
        Check.Fails(() => item.MoveTo(TrainingDuePlanStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TrainingDuePlanStartsInDraft();
        TrainingDuePlanMovesToActive();
        TrainingDuePlanRejectsMovesFromClosed();
    }
}
