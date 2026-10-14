namespace Acme.Workforce.Talent.Tests.SkillProfile.Forecast;

using Acme.Workforce.Talent.SkillProfile.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class DefaultSkillProfileForecastViewChecks
{
    public static void DefaultSkillProfileForecastViewShowsTheStatusInCapitals()
    {
        var item = DefaultSkillProfileForecast.NewDefaultSkillProfileForecast("code-933", "label-502");
        var view = DefaultSkillProfileForecastView.ToDefaultSkillProfileForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultSkillProfileForecastView.ToDefaultSkillProfileForecastViews(new List<DefaultSkillProfileForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultSkillProfileForecastViewShowsTheStatusInCapitals();
    }
}
