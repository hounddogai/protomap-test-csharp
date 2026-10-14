namespace Acme.Workforce.Absence.Tests.Holiday.Milestone;

using Acme.Workforce.Absence.Holiday.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class HolidayMilestoneChecks
{
    public static void HolidayMilestoneStartsInNew()
    {
        var item = HolidayMilestone.NewHolidayMilestone("code-918", "label-971");
        Check.Equal(item.Status, HolidayMilestoneStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayMilestoneMovesToFailed()
    {
        var item = HolidayMilestone.NewHolidayMilestone("code-918", "label-971");
        try
        {
            item.MoveTo(HolidayMilestoneStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayMilestoneStatus.Failed, "status");
    }

    public static void HolidayMilestoneRejectsMovesFromFailed()
    {
        var item = HolidayMilestone.NewHolidayMilestone("code-918", "label-971");
        item.Status = HolidayMilestoneStatus.Failed;
        Check.Fails(() => item.MoveTo(HolidayMilestoneStatus.New), "move");
    }

    public static void RunAll()
    {
        HolidayMilestoneStartsInNew();
        HolidayMilestoneMovesToFailed();
        HolidayMilestoneRejectsMovesFromFailed();
    }
}
