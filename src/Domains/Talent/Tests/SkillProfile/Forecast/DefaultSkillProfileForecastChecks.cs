namespace Acme.Workforce.Talent.Tests.SkillProfile.Forecast;

using Acme.Workforce.Talent.SkillProfile.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class DefaultSkillProfileForecastChecks
{
    public static void DefaultSkillProfileForecastStartsInPlanned()
    {
        var item = DefaultSkillProfileForecast.NewDefaultSkillProfileForecast("code-933", "label-502");
        Check.Equal(item.Status, DefaultSkillProfileForecastStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultSkillProfileForecastMovesToScheduled()
    {
        var item = DefaultSkillProfileForecast.NewDefaultSkillProfileForecast("code-933", "label-502");
        try
        {
            item.MoveTo(DefaultSkillProfileForecastStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultSkillProfileForecastStatus.Scheduled, "status");
    }

    public static void DefaultSkillProfileForecastRejectsMovesFromCancelled()
    {
        var item = DefaultSkillProfileForecast.NewDefaultSkillProfileForecast("code-933", "label-502");
        item.Status = DefaultSkillProfileForecastStatus.Cancelled;
        Check.Fails(() => item.MoveTo(DefaultSkillProfileForecastStatus.Planned), "move");
    }

    public static void RunAll()
    {
        DefaultSkillProfileForecastStartsInPlanned();
        DefaultSkillProfileForecastMovesToScheduled();
        DefaultSkillProfileForecastRejectsMovesFromCancelled();
    }
}
