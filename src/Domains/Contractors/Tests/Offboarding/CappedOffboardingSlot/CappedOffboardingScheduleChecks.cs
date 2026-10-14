namespace Acme.Workforce.Contractors.Tests.Offboarding.CappedOffboardingSlot;

using Acme.Workforce.Contractors.Offboarding.CappedOffboardingSlot;

/// <summary>Checks of capped offboarding slot code, run by the project's runner.</summary>
public static class CappedOffboardingScheduleChecks
{
    public static void CappedOffboardingScheduleStartsInRequested()
    {
        var item = CappedOffboardingSchedule.NewCappedOffboardingSchedule("code-80", "label-632");
        Check.Equal(item.Status, CappedOffboardingScheduleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedOffboardingScheduleMovesToGranted()
    {
        var item = CappedOffboardingSchedule.NewCappedOffboardingSchedule("code-80", "label-632");
        try
        {
            item.MoveTo(CappedOffboardingScheduleStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedOffboardingScheduleStatus.Granted, "status");
    }

    public static void CappedOffboardingScheduleRejectsMovesFromExpired()
    {
        var item = CappedOffboardingSchedule.NewCappedOffboardingSchedule("code-80", "label-632");
        item.Status = CappedOffboardingScheduleStatus.Expired;
        Check.Fails(() => item.MoveTo(CappedOffboardingScheduleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        CappedOffboardingScheduleStartsInRequested();
        CappedOffboardingScheduleMovesToGranted();
        CappedOffboardingScheduleRejectsMovesFromExpired();
    }
}
