namespace Acme.Workforce.Workplace.Tests.Visitor.Schedule;

using Acme.Workforce.Workplace.Visitor.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class VisitorScheduleChecks
{
    public static void VisitorScheduleStartsInRequested()
    {
        var item = VisitorSchedule.NewVisitorSchedule("code-38", "label-514");
        Check.Equal(item.Status, VisitorScheduleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorScheduleMovesToGranted()
    {
        var item = VisitorSchedule.NewVisitorSchedule("code-38", "label-514");
        try
        {
            item.MoveTo(VisitorScheduleStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorScheduleStatus.Granted, "status");
    }

    public static void VisitorScheduleRejectsMovesFromExpired()
    {
        var item = VisitorSchedule.NewVisitorSchedule("code-38", "label-514");
        item.Status = VisitorScheduleStatus.Expired;
        Check.Fails(() => item.MoveTo(VisitorScheduleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        VisitorScheduleStartsInRequested();
        VisitorScheduleMovesToGranted();
        VisitorScheduleRejectsMovesFromExpired();
    }
}
