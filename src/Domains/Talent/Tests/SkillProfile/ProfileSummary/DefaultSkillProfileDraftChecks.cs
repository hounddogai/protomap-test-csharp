namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileSummary;

using Acme.Workforce.Talent.SkillProfile.ProfileSummary;

/// <summary>Checks of profile summary code, run by the project's runner.</summary>
public static class DefaultSkillProfileDraftChecks
{
    public static void DefaultSkillProfileDraftStartsInRequested()
    {
        var item = DefaultSkillProfileDraft.NewDefaultSkillProfileDraft("code-307", "name-985");
        Check.Equal(item.Status, DefaultSkillProfileDraftStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultSkillProfileDraftMovesToRevoked()
    {
        var item = DefaultSkillProfileDraft.NewDefaultSkillProfileDraft("code-307", "name-985");
        try
        {
            item.MoveTo(DefaultSkillProfileDraftStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultSkillProfileDraftStatus.Revoked, "status");
    }

    public static void DefaultSkillProfileDraftRejectsMovesFromExpired()
    {
        var item = DefaultSkillProfileDraft.NewDefaultSkillProfileDraft("code-307", "name-985");
        item.Status = DefaultSkillProfileDraftStatus.Expired;
        Check.Fails(() => item.MoveTo(DefaultSkillProfileDraftStatus.Requested), "move");
    }

    public static void RunAll()
    {
        DefaultSkillProfileDraftStartsInRequested();
        DefaultSkillProfileDraftMovesToRevoked();
        DefaultSkillProfileDraftRejectsMovesFromExpired();
    }
}
