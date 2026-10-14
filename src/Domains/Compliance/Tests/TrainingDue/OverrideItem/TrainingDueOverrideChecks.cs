namespace Acme.Workforce.Compliance.Tests.TrainingDue.OverrideItem;

using Acme.Workforce.Compliance.TrainingDue.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class TrainingDueOverrideChecks
{
    public static void TrainingDueOverrideStartsInOpen()
    {
        var item = TrainingDueOverride.NewTrainingDueOverride("code-557", "label-146");
        Check.Equal(item.Status, TrainingDueOverrideStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDueOverrideMovesToArchived()
    {
        var item = TrainingDueOverride.NewTrainingDueOverride("code-557", "label-146");
        try
        {
            item.MoveTo(TrainingDueOverrideStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDueOverrideStatus.Archived, "status");
    }

    public static void TrainingDueOverrideRejectsMovesFromArchived()
    {
        var item = TrainingDueOverride.NewTrainingDueOverride("code-557", "label-146");
        item.Status = TrainingDueOverrideStatus.Archived;
        Check.Fails(() => item.MoveTo(TrainingDueOverrideStatus.Open), "move");
    }

    public static void RunAll()
    {
        TrainingDueOverrideStartsInOpen();
        TrainingDueOverrideMovesToArchived();
        TrainingDueOverrideRejectsMovesFromArchived();
    }
}
