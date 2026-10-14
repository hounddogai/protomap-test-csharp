namespace Acme.Workforce.Absence.Tests.ReturnPlan.Bundle;

using Acme.Workforce.Absence.ReturnPlan.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class ReturnPlanBundleViewChecks
{
    public static void ReturnPlanBundleViewShowsTheStatusInCapitals()
    {
        var item = ReturnPlanBundle.NewReturnPlanBundle("code-189", "name-542");
        var view = ReturnPlanBundleView.ToReturnPlanBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReturnPlanBundleView.ToReturnPlanBundleViews(new List<ReturnPlanBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReturnPlanBundleViewShowsTheStatusInCapitals();
    }
}
