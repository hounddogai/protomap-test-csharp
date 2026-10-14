namespace Acme.Workforce.Compliance.Tests.TrainingDue.Notice;

using Acme.Workforce.Compliance.TrainingDue.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class TrainingDueNoticeChecks
{
    public static void TrainingDueNoticeStartsInPlanned()
    {
        var item = TrainingDueNotice.NewTrainingDueNotice("code-101", "label-714");
        Check.Equal(item.Status, TrainingDueNoticeStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDueNoticeMovesToScheduled()
    {
        var item = TrainingDueNotice.NewTrainingDueNotice("code-101", "label-714");
        try
        {
            item.MoveTo(TrainingDueNoticeStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDueNoticeStatus.Scheduled, "status");
    }

    public static void TrainingDueNoticeRejectsMovesFromCancelled()
    {
        var item = TrainingDueNotice.NewTrainingDueNotice("code-101", "label-714");
        item.Status = TrainingDueNoticeStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TrainingDueNoticeStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TrainingDueNoticeStartsInPlanned();
        TrainingDueNoticeMovesToScheduled();
        TrainingDueNoticeRejectsMovesFromCancelled();
    }
}
