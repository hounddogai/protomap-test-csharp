namespace Acme.Workforce.Talent.Tests.CareerPath.PathDigest;

using Acme.Workforce.Talent.CareerPath.PathDigest;

/// <summary>Checks of path digest code, run by the project's runner.</summary>
public static class RollingCareerPathForecastViewChecks
{
    public static void RollingCareerPathForecastViewShowsTheStatusInCapitals()
    {
        var item = RollingCareerPathForecast.NewRollingCareerPathForecast("code-502", "title-376");
        var view = RollingCareerPathForecastView.ToRollingCareerPathForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingCareerPathForecastView.ToRollingCareerPathForecastViews(new List<RollingCareerPathForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingCareerPathForecastViewShowsTheStatusInCapitals();
    }
}
