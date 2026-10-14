namespace Acme.Workforce.Talent.Tests.SkillProfile.Policy;

using Acme.Workforce.Talent.SkillProfile.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class SkillProfileGroupChecks
{
    public static void SkillProfileGroupStartsInOpen()
    {
        var item = SkillProfileGroup.NewSkillProfileGroup("code-986", "name-326");
        Check.Equal(item.Status, SkillProfileGroupStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SkillProfileGroupMovesToInReview()
    {
        var item = SkillProfileGroup.NewSkillProfileGroup("code-986", "name-326");
        try
        {
            item.MoveTo(SkillProfileGroupStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SkillProfileGroupStatus.InReview, "status");
    }

    public static void SkillProfileGroupRejectsMovesFromArchived()
    {
        var item = SkillProfileGroup.NewSkillProfileGroup("code-986", "name-326");
        item.Status = SkillProfileGroupStatus.Archived;
        Check.Fails(() => item.MoveTo(SkillProfileGroupStatus.Open), "move");
    }

    public static void RunAll()
    {
        SkillProfileGroupStartsInOpen();
        SkillProfileGroupMovesToInReview();
        SkillProfileGroupRejectsMovesFromArchived();
    }
}
