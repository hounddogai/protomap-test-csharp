namespace Acme.Workforce.Absence.Tests.Holiday.Allocation;

using Acme.Workforce.Absence.Holiday.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class HolidayAllocationViewChecks
{
    public static void HolidayAllocationViewShowsTheStatusInCapitals()
    {
        var item = HolidayAllocation.NewHolidayAllocation("code-320", "name-777");
        var view = HolidayAllocationView.ToHolidayAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HolidayAllocationView.ToHolidayAllocationViews(new List<HolidayAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HolidayAllocationViewShowsTheStatusInCapitals();
    }
}
