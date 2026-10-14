namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueSchedule;

using Acme.Workforce.Compliance.TrainingDue.DueSchedule;

/// <summary>Checks of due schedule code, run by the project's runner.</summary>
public static class RegionalTrainingDueNoticeChecks
{
    public static void RegionalTrainingDueNoticeStartsInRequested()
    {
        var item = RegionalTrainingDueNotice.NewRegionalTrainingDueNotice("code-917", "label-374");
        Check.Equal(item.Status, RegionalTrainingDueNoticeStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalTrainingDueNoticeMovesToGranted()
    {
        var item = RegionalTrainingDueNotice.NewRegionalTrainingDueNotice("code-917", "label-374");
        try
        {
            item.MoveTo(RegionalTrainingDueNoticeStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalTrainingDueNoticeStatus.Granted, "status");
    }

    public static void RegionalTrainingDueNoticeRejectsMovesFromExpired()
    {
        var item = RegionalTrainingDueNotice.NewRegionalTrainingDueNotice("code-917", "label-374");
        item.Status = RegionalTrainingDueNoticeStatus.Expired;
        Check.Fails(() => item.MoveTo(RegionalTrainingDueNoticeStatus.Requested), "move");
    }

    public static void RunAll()
    {
        RegionalTrainingDueNoticeStartsInRequested();
        RegionalTrainingDueNoticeMovesToGranted();
        RegionalTrainingDueNoticeRejectsMovesFromExpired();
    }
}
