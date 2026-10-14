namespace Acme.Workforce.Absence.Tests.ReturnPlan.Preference;

using Acme.Workforce.Absence.ReturnPlan.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ReturnPlanProfileViewChecks
{
    public static void ReturnPlanProfileViewShowsTheStatusInCapitals()
    {
        var item = ReturnPlanProfile.NewReturnPlanProfile("code-863", "name-483");
        var view = ReturnPlanProfileView.ToReturnPlanProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReturnPlanProfileView.ToReturnPlanProfileViews(new List<ReturnPlanProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReturnPlanProfileViewShowsTheStatusInCapitals();
    }
}
