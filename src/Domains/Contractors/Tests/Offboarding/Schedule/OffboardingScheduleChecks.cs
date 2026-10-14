namespace Acme.Workforce.Contractors.Tests.Offboarding.Schedule;

using Acme.Workforce.Contractors.Offboarding.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class OffboardingScheduleChecks
{
    public static void OffboardingScheduleStartsInDraft()
    {
        var item = OffboardingSchedule.NewOffboardingSchedule("code-441", "title-328");
        Check.Equal(item.Status, OffboardingScheduleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OffboardingScheduleMovesToClosed()
    {
        var item = OffboardingSchedule.NewOffboardingSchedule("code-441", "title-328");
        try
        {
            item.MoveTo(OffboardingScheduleStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OffboardingScheduleStatus.Closed, "status");
    }

    public static void OffboardingScheduleRejectsMovesFromClosed()
    {
        var item = OffboardingSchedule.NewOffboardingSchedule("code-441", "title-328");
        item.Status = OffboardingScheduleStatus.Closed;
        Check.Fails(() => item.MoveTo(OffboardingScheduleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        OffboardingScheduleStartsInDraft();
        OffboardingScheduleMovesToClosed();
        OffboardingScheduleRejectsMovesFromClosed();
    }
}
