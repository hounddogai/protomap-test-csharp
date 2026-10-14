namespace Acme.Workforce.Talent.Tests.CareerPath.Plan;

using Acme.Workforce.Talent.CareerPath.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class WeightedCareerPathOverrideViewChecks
{
    public static void WeightedCareerPathOverrideViewShowsTheStatusInCapitals()
    {
        var item = WeightedCareerPathOverride.NewWeightedCareerPathOverride("code-915", "title-538");
        var view = WeightedCareerPathOverrideView.ToWeightedCareerPathOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedCareerPathOverrideView.ToWeightedCareerPathOverrideViews(new List<WeightedCareerPathOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedCareerPathOverrideViewShowsTheStatusInCapitals();
    }
}
