namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Request;

using Acme.Workforce.Talent.SuccessionPlan.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class GlobalSuccessionPlanRequestViewChecks
{
    public static void GlobalSuccessionPlanRequestViewShowsTheStatusInCapitals()
    {
        var item = GlobalSuccessionPlanRequest.NewGlobalSuccessionPlanRequest("code-808", "label-269");
        var view = GlobalSuccessionPlanRequestView.ToGlobalSuccessionPlanRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalSuccessionPlanRequestView.ToGlobalSuccessionPlanRequestViews(new List<GlobalSuccessionPlanRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalSuccessionPlanRequestViewShowsTheStatusInCapitals();
    }
}
