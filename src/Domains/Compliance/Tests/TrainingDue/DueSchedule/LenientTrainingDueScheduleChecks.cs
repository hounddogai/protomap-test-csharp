namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueSchedule;

using Acme.Workforce.Compliance.TrainingDue.DueSchedule;

/// <summary>Checks of due schedule code, run by the project's runner.</summary>
public static class LenientTrainingDueScheduleChecks
{
    public static void LenientTrainingDueScheduleStartsInPlanned()
    {
        var item = LenientTrainingDueSchedule.NewLenientTrainingDueSchedule("code-648", "label-466");
        Check.Equal(item.Status, LenientTrainingDueScheduleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientTrainingDueScheduleMovesToScheduled()
    {
        var item = LenientTrainingDueSchedule.NewLenientTrainingDueSchedule("code-648", "label-466");
        try
        {
            item.MoveTo(LenientTrainingDueScheduleStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientTrainingDueScheduleStatus.Scheduled, "status");
    }

    public static void LenientTrainingDueScheduleRejectsMovesFromCancelled()
    {
        var item = LenientTrainingDueSchedule.NewLenientTrainingDueSchedule("code-648", "label-466");
        item.Status = LenientTrainingDueScheduleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LenientTrainingDueScheduleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LenientTrainingDueScheduleStartsInPlanned();
        LenientTrainingDueScheduleMovesToScheduled();
        LenientTrainingDueScheduleRejectsMovesFromCancelled();
    }
}
