namespace Acme.Workforce.Talent.Tests.CareerPath.Metric;

using Acme.Workforce.Talent.CareerPath.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CareerPathPolicyViewChecks
{
    public static void CareerPathPolicyViewShowsTheStatusInCapitals()
    {
        var item = CareerPathPolicy.NewCareerPathPolicy("code-309", "label-28");
        var view = CareerPathPolicyView.ToCareerPathPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CareerPathPolicyView.ToCareerPathPolicyViews(new List<CareerPathPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CareerPathPolicyViewShowsTheStatusInCapitals();
    }
}
