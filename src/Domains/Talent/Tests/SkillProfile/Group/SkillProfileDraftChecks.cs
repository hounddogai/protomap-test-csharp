namespace Acme.Workforce.Talent.Tests.SkillProfile.Group;

using Acme.Workforce.Talent.SkillProfile.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class SkillProfileDraftChecks
{
    public static void SkillProfileDraftStartsInProposed()
    {
        var item = SkillProfileDraft.NewSkillProfileDraft("code-718", "name-811");
        Check.Equal(item.Status, SkillProfileDraftStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SkillProfileDraftMovesToWithdrawn()
    {
        var item = SkillProfileDraft.NewSkillProfileDraft("code-718", "name-811");
        try
        {
            item.MoveTo(SkillProfileDraftStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SkillProfileDraftStatus.Withdrawn, "status");
    }

    public static void SkillProfileDraftRejectsMovesFromWithdrawn()
    {
        var item = SkillProfileDraft.NewSkillProfileDraft("code-718", "name-811");
        item.Status = SkillProfileDraftStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(SkillProfileDraftStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        SkillProfileDraftStartsInProposed();
        SkillProfileDraftMovesToWithdrawn();
        SkillProfileDraftRejectsMovesFromWithdrawn();
    }
}
