namespace Acme.Workforce.Absence.Tests.Holiday.Policy;

using Acme.Workforce.Absence.Holiday.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class HolidayPolicyViewChecks
{
    public static void HolidayPolicyViewShowsTheStatusInCapitals()
    {
        var item = HolidayPolicy.NewHolidayPolicy("code-111", "name-247");
        var view = HolidayPolicyView.ToHolidayPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HolidayPolicyView.ToHolidayPolicyViews(new List<HolidayPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HolidayPolicyViewShowsTheStatusInCapitals();
    }
}
