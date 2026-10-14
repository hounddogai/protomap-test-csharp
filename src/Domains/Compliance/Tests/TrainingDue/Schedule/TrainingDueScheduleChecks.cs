namespace Acme.Workforce.Compliance.Tests.TrainingDue.Schedule;

using Acme.Workforce.Compliance.TrainingDue.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class TrainingDueScheduleChecks
{
    public static void TrainingDueScheduleStartsInOpen()
    {
        var item = TrainingDueSchedule.NewTrainingDueSchedule("code-375", "title-726");
        Check.Equal(item.Status, TrainingDueScheduleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDueScheduleMovesToResolved()
    {
        var item = TrainingDueSchedule.NewTrainingDueSchedule("code-375", "title-726");
        try
        {
            item.MoveTo(TrainingDueScheduleStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDueScheduleStatus.Resolved, "status");
    }

    public static void TrainingDueScheduleRejectsMovesFromArchived()
    {
        var item = TrainingDueSchedule.NewTrainingDueSchedule("code-375", "title-726");
        item.Status = TrainingDueScheduleStatus.Archived;
        Check.Fails(() => item.MoveTo(TrainingDueScheduleStatus.Open), "move");
    }

    public static void RunAll()
    {
        TrainingDueScheduleStartsInOpen();
        TrainingDueScheduleMovesToResolved();
        TrainingDueScheduleRejectsMovesFromArchived();
    }
}
