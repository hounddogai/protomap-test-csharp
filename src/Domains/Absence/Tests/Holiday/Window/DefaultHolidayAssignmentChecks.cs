namespace Acme.Workforce.Absence.Tests.Holiday.Window;

using Acme.Workforce.Absence.Holiday.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class DefaultHolidayAssignmentChecks
{
    public static void DefaultHolidayAssignmentStartsInNew()
    {
        var item = DefaultHolidayAssignment.NewDefaultHolidayAssignment("code-155", "title-450");
        Check.Equal(item.Status, DefaultHolidayAssignmentStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultHolidayAssignmentMovesToQueued()
    {
        var item = DefaultHolidayAssignment.NewDefaultHolidayAssignment("code-155", "title-450");
        try
        {
            item.MoveTo(DefaultHolidayAssignmentStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultHolidayAssignmentStatus.Queued, "status");
    }

    public static void DefaultHolidayAssignmentRejectsMovesFromFailed()
    {
        var item = DefaultHolidayAssignment.NewDefaultHolidayAssignment("code-155", "title-450");
        item.Status = DefaultHolidayAssignmentStatus.Failed;
        Check.Fails(() => item.MoveTo(DefaultHolidayAssignmentStatus.New), "move");
    }

    public static void RunAll()
    {
        DefaultHolidayAssignmentStartsInNew();
        DefaultHolidayAssignmentMovesToQueued();
        DefaultHolidayAssignmentRejectsMovesFromFailed();
    }
}
