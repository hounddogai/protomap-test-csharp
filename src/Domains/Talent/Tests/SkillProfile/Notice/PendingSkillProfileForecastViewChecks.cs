namespace Acme.Workforce.Talent.Tests.SkillProfile.Notice;

using Acme.Workforce.Talent.SkillProfile.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class PendingSkillProfileForecastViewChecks
{
    public static void PendingSkillProfileForecastViewShowsTheStatusInCapitals()
    {
        var item = PendingSkillProfileForecast.NewPendingSkillProfileForecast("code-786", "title-360");
        var view = PendingSkillProfileForecastView.ToPendingSkillProfileForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingSkillProfileForecastView.ToPendingSkillProfileForecastViews(new List<PendingSkillProfileForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingSkillProfileForecastViewShowsTheStatusInCapitals();
    }
}
