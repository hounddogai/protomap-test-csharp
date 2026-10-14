namespace Acme.Workforce.Compliance.Tests.TrainingDue.Mapping;

using Acme.Workforce.Compliance.TrainingDue.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class TrainingDueMappingChecks
{
    public static void TrainingDueMappingStartsInPlanned()
    {
        var item = TrainingDueMapping.NewTrainingDueMapping("code-687", "name-57");
        Check.Equal(item.Status, TrainingDueMappingStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDueMappingMovesToInProgress()
    {
        var item = TrainingDueMapping.NewTrainingDueMapping("code-687", "name-57");
        try
        {
            item.MoveTo(TrainingDueMappingStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDueMappingStatus.InProgress, "status");
    }

    public static void TrainingDueMappingRejectsMovesFromCancelled()
    {
        var item = TrainingDueMapping.NewTrainingDueMapping("code-687", "name-57");
        item.Status = TrainingDueMappingStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TrainingDueMappingStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TrainingDueMappingStartsInPlanned();
        TrainingDueMappingMovesToInProgress();
        TrainingDueMappingRejectsMovesFromCancelled();
    }
}
