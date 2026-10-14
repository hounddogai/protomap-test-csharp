namespace Acme.Workforce.Talent.Tests.CareerPath.Plan;

using Acme.Workforce.Talent.CareerPath.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class CareerPathPlanViewChecks
{
    public static void CareerPathPlanViewShowsTheStatusInCapitals()
    {
        var item = CareerPathPlan.NewCareerPathPlan("code-260", "label-769");
        var view = CareerPathPlanView.ToCareerPathPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CareerPathPlanView.ToCareerPathPlanViews(new List<CareerPathPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CareerPathPlanViewShowsTheStatusInCapitals();
    }
}
