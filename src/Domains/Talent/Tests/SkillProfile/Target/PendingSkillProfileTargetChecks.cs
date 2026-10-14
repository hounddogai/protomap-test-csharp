namespace Acme.Workforce.Talent.Tests.SkillProfile.Target;

using Acme.Workforce.Talent.SkillProfile.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PendingSkillProfileTargetChecks
{
    public static void PendingSkillProfileTargetStartsInRequested()
    {
        var item = PendingSkillProfileTarget.NewPendingSkillProfileTarget("code-46", "name-390");
        Check.Equal(item.Status, PendingSkillProfileTargetStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingSkillProfileTargetMovesToExpired()
    {
        var item = PendingSkillProfileTarget.NewPendingSkillProfileTarget("code-46", "name-390");
        try
        {
            item.MoveTo(PendingSkillProfileTargetStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingSkillProfileTargetStatus.Expired, "status");
    }

    public static void PendingSkillProfileTargetRejectsMovesFromExpired()
    {
        var item = PendingSkillProfileTarget.NewPendingSkillProfileTarget("code-46", "name-390");
        item.Status = PendingSkillProfileTargetStatus.Expired;
        Check.Fails(() => item.MoveTo(PendingSkillProfileTargetStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PendingSkillProfileTargetStartsInRequested();
        PendingSkillProfileTargetMovesToExpired();
        PendingSkillProfileTargetRejectsMovesFromExpired();
    }
}
