namespace Acme.Workforce.Talent.Tests.SkillProfile.Notice;

using Acme.Workforce.Talent.SkillProfile.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class PendingSkillProfileForecastChecks
{
    public static void PendingSkillProfileForecastStartsInPlanned()
    {
        var item = PendingSkillProfileForecast.NewPendingSkillProfileForecast("code-786", "title-360");
        Check.Equal(item.Status, PendingSkillProfileForecastStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingSkillProfileForecastMovesToScheduled()
    {
        var item = PendingSkillProfileForecast.NewPendingSkillProfileForecast("code-786", "title-360");
        try
        {
            item.MoveTo(PendingSkillProfileForecastStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingSkillProfileForecastStatus.Scheduled, "status");
    }

    public static void PendingSkillProfileForecastRejectsMovesFromCancelled()
    {
        var item = PendingSkillProfileForecast.NewPendingSkillProfileForecast("code-786", "title-360");
        item.Status = PendingSkillProfileForecastStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PendingSkillProfileForecastStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PendingSkillProfileForecastStartsInPlanned();
        PendingSkillProfileForecastMovesToScheduled();
        PendingSkillProfileForecastRejectsMovesFromCancelled();
    }
}
