namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Window;

using Acme.Workforce.Talent.SuccessionPlan.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class SuccessionPlanWindowViewChecks
{
    public static void SuccessionPlanWindowViewShowsTheStatusInCapitals()
    {
        var item = SuccessionPlanWindow.NewSuccessionPlanWindow("code-605", "label-213");
        var view = SuccessionPlanWindowView.ToSuccessionPlanWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SuccessionPlanWindowView.ToSuccessionPlanWindowViews(new List<SuccessionPlanWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SuccessionPlanWindowViewShowsTheStatusInCapitals();
    }
}
