namespace Acme.Workforce.Absence.Tests.Holiday.Policy;

using Acme.Workforce.Absence.Holiday.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class HolidayRuleViewChecks
{
    public static void HolidayRuleViewShowsTheStatusInCapitals()
    {
        var item = HolidayRule.NewHolidayRule("code-152", "title-551");
        var view = HolidayRuleView.ToHolidayRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HolidayRuleView.ToHolidayRuleViews(new List<HolidayRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HolidayRuleViewShowsTheStatusInCapitals();
    }
}
