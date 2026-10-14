namespace Acme.Workforce.Absence.Tests.Holiday.Metric;

using Acme.Workforce.Absence.Holiday.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class DefaultHolidayMilestoneChecks
{
    public static void DefaultHolidayMilestoneStartsInRequested()
    {
        var item = DefaultHolidayMilestone.NewDefaultHolidayMilestone("code-588", "label-452");
        Check.Equal(item.Status, DefaultHolidayMilestoneStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultHolidayMilestoneMovesToGranted()
    {
        var item = DefaultHolidayMilestone.NewDefaultHolidayMilestone("code-588", "label-452");
        try
        {
            item.MoveTo(DefaultHolidayMilestoneStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultHolidayMilestoneStatus.Granted, "status");
    }

    public static void DefaultHolidayMilestoneRejectsMovesFromExpired()
    {
        var item = DefaultHolidayMilestone.NewDefaultHolidayMilestone("code-588", "label-452");
        item.Status = DefaultHolidayMilestoneStatus.Expired;
        Check.Fails(() => item.MoveTo(DefaultHolidayMilestoneStatus.Requested), "move");
    }

    public static void RunAll()
    {
        DefaultHolidayMilestoneStartsInRequested();
        DefaultHolidayMilestoneMovesToGranted();
        DefaultHolidayMilestoneRejectsMovesFromExpired();
    }
}
