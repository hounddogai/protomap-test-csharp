namespace Acme.Workforce.Talent.Tests.SkillProfile.Preference;

using Acme.Workforce.Talent.SkillProfile.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class LegacySkillProfilePreferenceChecks
{
    public static void LegacySkillProfilePreferenceStartsInRequested()
    {
        var item = LegacySkillProfilePreference.NewLegacySkillProfilePreference("code-704", "name-17");
        Check.Equal(item.Status, LegacySkillProfilePreferenceStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacySkillProfilePreferenceMovesToGranted()
    {
        var item = LegacySkillProfilePreference.NewLegacySkillProfilePreference("code-704", "name-17");
        try
        {
            item.MoveTo(LegacySkillProfilePreferenceStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacySkillProfilePreferenceStatus.Granted, "status");
    }

    public static void LegacySkillProfilePreferenceRejectsMovesFromExpired()
    {
        var item = LegacySkillProfilePreference.NewLegacySkillProfilePreference("code-704", "name-17");
        item.Status = LegacySkillProfilePreferenceStatus.Expired;
        Check.Fails(() => item.MoveTo(LegacySkillProfilePreferenceStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LegacySkillProfilePreferenceStartsInRequested();
        LegacySkillProfilePreferenceMovesToGranted();
        LegacySkillProfilePreferenceRejectsMovesFromExpired();
    }
}
