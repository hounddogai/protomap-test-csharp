namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileGroup;

using Acme.Workforce.Talent.SkillProfile.ProfileGroup;

/// <summary>Checks of profile group code, run by the project's runner.</summary>
public static class DefaultSkillProfileGroupChecks
{
    public static void DefaultSkillProfileGroupStartsInRequested()
    {
        var item = DefaultSkillProfileGroup.NewDefaultSkillProfileGroup("code-197", "label-335");
        Check.Equal(item.Status, DefaultSkillProfileGroupStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultSkillProfileGroupMovesToGranted()
    {
        var item = DefaultSkillProfileGroup.NewDefaultSkillProfileGroup("code-197", "label-335");
        try
        {
            item.MoveTo(DefaultSkillProfileGroupStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultSkillProfileGroupStatus.Granted, "status");
    }

    public static void DefaultSkillProfileGroupRejectsMovesFromExpired()
    {
        var item = DefaultSkillProfileGroup.NewDefaultSkillProfileGroup("code-197", "label-335");
        item.Status = DefaultSkillProfileGroupStatus.Expired;
        Check.Fails(() => item.MoveTo(DefaultSkillProfileGroupStatus.Requested), "move");
    }

    public static void RunAll()
    {
        DefaultSkillProfileGroupStartsInRequested();
        DefaultSkillProfileGroupMovesToGranted();
        DefaultSkillProfileGroupRejectsMovesFromExpired();
    }
}
