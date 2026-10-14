namespace Acme.Workforce.Talent.Tests.SkillProfile.Review;

using Acme.Workforce.Talent.SkillProfile.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class CappedSkillProfileForecastChecks
{
    public static void CappedSkillProfileForecastStartsInRequested()
    {
        var item = CappedSkillProfileForecast.NewCappedSkillProfileForecast("code-533", "label-88");
        Check.Equal(item.Status, CappedSkillProfileForecastStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedSkillProfileForecastMovesToRevoked()
    {
        var item = CappedSkillProfileForecast.NewCappedSkillProfileForecast("code-533", "label-88");
        try
        {
            item.MoveTo(CappedSkillProfileForecastStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedSkillProfileForecastStatus.Revoked, "status");
    }

    public static void CappedSkillProfileForecastRejectsMovesFromExpired()
    {
        var item = CappedSkillProfileForecast.NewCappedSkillProfileForecast("code-533", "label-88");
        item.Status = CappedSkillProfileForecastStatus.Expired;
        Check.Fails(() => item.MoveTo(CappedSkillProfileForecastStatus.Requested), "move");
    }

    public static void RunAll()
    {
        CappedSkillProfileForecastStartsInRequested();
        CappedSkillProfileForecastMovesToRevoked();
        CappedSkillProfileForecastRejectsMovesFromExpired();
    }
}
