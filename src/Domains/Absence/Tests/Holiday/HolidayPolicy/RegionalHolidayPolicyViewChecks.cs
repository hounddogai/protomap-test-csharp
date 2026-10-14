namespace Acme.Workforce.Absence.Tests.Holiday.HolidayPolicy;

using Acme.Workforce.Absence.Holiday.HolidayPolicy;

/// <summary>Checks of holiday policy code, run by the project's runner.</summary>
public static class RegionalHolidayPolicyViewChecks
{
    public static void RegionalHolidayPolicyViewShowsTheStatusInCapitals()
    {
        var item = RegionalHolidayPolicy.NewRegionalHolidayPolicy("code-585", "name-273");
        var view = RegionalHolidayPolicyView.ToRegionalHolidayPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalHolidayPolicyView.ToRegionalHolidayPolicyViews(new List<RegionalHolidayPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalHolidayPolicyViewShowsTheStatusInCapitals();
    }
}
