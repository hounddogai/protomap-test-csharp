namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileGroup;

using Acme.Workforce.Talent.SkillProfile.ProfileGroup;

/// <summary>Checks of profile group code, run by the project's runner.</summary>
public static class SeasonalSkillProfilePreferenceChecks
{
    public static void SeasonalSkillProfilePreferenceStartsInPlanned()
    {
        var item = SeasonalSkillProfilePreference.NewSeasonalSkillProfilePreference("code-293", "title-427");
        Check.Equal(item.Status, SeasonalSkillProfilePreferenceStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalSkillProfilePreferenceMovesToCancelled()
    {
        var item = SeasonalSkillProfilePreference.NewSeasonalSkillProfilePreference("code-293", "title-427");
        try
        {
            item.MoveTo(SeasonalSkillProfilePreferenceStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalSkillProfilePreferenceStatus.Cancelled, "status");
    }

    public static void SeasonalSkillProfilePreferenceRejectsMovesFromCancelled()
    {
        var item = SeasonalSkillProfilePreference.NewSeasonalSkillProfilePreference("code-293", "title-427");
        item.Status = SeasonalSkillProfilePreferenceStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SeasonalSkillProfilePreferenceStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SeasonalSkillProfilePreferenceStartsInPlanned();
        SeasonalSkillProfilePreferenceMovesToCancelled();
        SeasonalSkillProfilePreferenceRejectsMovesFromCancelled();
    }
}
