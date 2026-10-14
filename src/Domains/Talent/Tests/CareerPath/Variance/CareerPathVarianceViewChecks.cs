namespace Acme.Workforce.Talent.Tests.CareerPath.Variance;

using Acme.Workforce.Talent.CareerPath.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class CareerPathVarianceViewChecks
{
    public static void CareerPathVarianceViewShowsTheStatusInCapitals()
    {
        var item = CareerPathVariance.NewCareerPathVariance("code-416", "title-157");
        var view = CareerPathVarianceView.ToCareerPathVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CareerPathVarianceView.ToCareerPathVarianceViews(new List<CareerPathVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CareerPathVarianceViewShowsTheStatusInCapitals();
    }
}
