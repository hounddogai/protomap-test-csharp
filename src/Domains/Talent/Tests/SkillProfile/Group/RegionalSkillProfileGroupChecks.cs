namespace Acme.Workforce.Talent.Tests.SkillProfile.Group;

using Acme.Workforce.Talent.SkillProfile.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class RegionalSkillProfileGroupChecks
{
    public static void RegionalSkillProfileGroupStartsInCreated()
    {
        var item = RegionalSkillProfileGroup.NewRegionalSkillProfileGroup("code-621", "label-205");
        Check.Equal(item.Status, RegionalSkillProfileGroupStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalSkillProfileGroupMovesToPublished()
    {
        var item = RegionalSkillProfileGroup.NewRegionalSkillProfileGroup("code-621", "label-205");
        try
        {
            item.MoveTo(RegionalSkillProfileGroupStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalSkillProfileGroupStatus.Published, "status");
    }

    public static void RegionalSkillProfileGroupRejectsMovesFromRetired()
    {
        var item = RegionalSkillProfileGroup.NewRegionalSkillProfileGroup("code-621", "label-205");
        item.Status = RegionalSkillProfileGroupStatus.Retired;
        Check.Fails(() => item.MoveTo(RegionalSkillProfileGroupStatus.Created), "move");
    }

    public static void RunAll()
    {
        RegionalSkillProfileGroupStartsInCreated();
        RegionalSkillProfileGroupMovesToPublished();
        RegionalSkillProfileGroupRejectsMovesFromRetired();
    }
}
