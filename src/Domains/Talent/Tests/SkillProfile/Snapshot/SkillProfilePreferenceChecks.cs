namespace Acme.Workforce.Talent.Tests.SkillProfile.Snapshot;

using Acme.Workforce.Talent.SkillProfile.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SkillProfilePreferenceChecks
{
    public static void SkillProfilePreferenceStartsInCreated()
    {
        var item = SkillProfilePreference.NewSkillProfilePreference("code-523", "label-901");
        Check.Equal(item.Status, SkillProfilePreferenceStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SkillProfilePreferenceMovesToPublished()
    {
        var item = SkillProfilePreference.NewSkillProfilePreference("code-523", "label-901");
        try
        {
            item.MoveTo(SkillProfilePreferenceStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SkillProfilePreferenceStatus.Published, "status");
    }

    public static void SkillProfilePreferenceRejectsMovesFromRetired()
    {
        var item = SkillProfilePreference.NewSkillProfilePreference("code-523", "label-901");
        item.Status = SkillProfilePreferenceStatus.Retired;
        Check.Fails(() => item.MoveTo(SkillProfilePreferenceStatus.Created), "move");
    }

    public static void RunAll()
    {
        SkillProfilePreferenceStartsInCreated();
        SkillProfilePreferenceMovesToPublished();
        SkillProfilePreferenceRejectsMovesFromRetired();
    }
}
