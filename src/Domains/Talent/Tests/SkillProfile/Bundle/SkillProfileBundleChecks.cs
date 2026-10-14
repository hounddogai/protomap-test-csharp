namespace Acme.Workforce.Talent.Tests.SkillProfile.Bundle;

using Acme.Workforce.Talent.SkillProfile.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class SkillProfileBundleChecks
{
    public static void SkillProfileBundleStartsInCreated()
    {
        var item = SkillProfileBundle.NewSkillProfileBundle("code-230", "label-715");
        Check.Equal(item.Status, SkillProfileBundleStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SkillProfileBundleMovesToValidated()
    {
        var item = SkillProfileBundle.NewSkillProfileBundle("code-230", "label-715");
        try
        {
            item.MoveTo(SkillProfileBundleStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SkillProfileBundleStatus.Validated, "status");
    }

    public static void SkillProfileBundleRejectsMovesFromRetired()
    {
        var item = SkillProfileBundle.NewSkillProfileBundle("code-230", "label-715");
        item.Status = SkillProfileBundleStatus.Retired;
        Check.Fails(() => item.MoveTo(SkillProfileBundleStatus.Created), "move");
    }

    public static void RunAll()
    {
        SkillProfileBundleStartsInCreated();
        SkillProfileBundleMovesToValidated();
        SkillProfileBundleRejectsMovesFromRetired();
    }
}
