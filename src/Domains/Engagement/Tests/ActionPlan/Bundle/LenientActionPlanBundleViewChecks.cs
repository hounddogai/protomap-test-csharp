namespace Acme.Workforce.Engagement.Tests.ActionPlan.Bundle;

using Acme.Workforce.Engagement.ActionPlan.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LenientActionPlanBundleViewChecks
{
    public static void LenientActionPlanBundleViewShowsTheStatusInCapitals()
    {
        var item = LenientActionPlanBundle.NewLenientActionPlanBundle("code-249", "name-741");
        var view = LenientActionPlanBundleView.ToLenientActionPlanBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientActionPlanBundleView.ToLenientActionPlanBundleViews(new List<LenientActionPlanBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientActionPlanBundleViewShowsTheStatusInCapitals();
    }
}
