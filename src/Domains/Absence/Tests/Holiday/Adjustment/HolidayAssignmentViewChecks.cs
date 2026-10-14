namespace Acme.Workforce.Absence.Tests.Holiday.Adjustment;

using Acme.Workforce.Absence.Holiday.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class HolidayAssignmentViewChecks
{
    public static void HolidayAssignmentViewShowsTheStatusInCapitals()
    {
        var item = HolidayAssignment.NewHolidayAssignment("code-105", "name-749");
        var view = HolidayAssignmentView.ToHolidayAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HolidayAssignmentView.ToHolidayAssignmentViews(new List<HolidayAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HolidayAssignmentViewShowsTheStatusInCapitals();
    }
}
