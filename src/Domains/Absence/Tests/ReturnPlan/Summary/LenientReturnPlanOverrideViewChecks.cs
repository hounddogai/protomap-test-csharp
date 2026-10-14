namespace Acme.Workforce.Absence.Tests.ReturnPlan.Summary;

using Acme.Workforce.Absence.ReturnPlan.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class LenientReturnPlanOverrideViewChecks
{
    public static void LenientReturnPlanOverrideViewShowsTheStatusInCapitals()
    {
        var item = LenientReturnPlanOverride.NewLenientReturnPlanOverride("code-554", "name-821");
        var view = LenientReturnPlanOverrideView.ToLenientReturnPlanOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientReturnPlanOverrideView.ToLenientReturnPlanOverrideViews(new List<LenientReturnPlanOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientReturnPlanOverrideViewShowsTheStatusInCapitals();
    }
}
