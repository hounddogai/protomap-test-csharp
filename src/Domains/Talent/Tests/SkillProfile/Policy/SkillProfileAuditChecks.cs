namespace Acme.Workforce.Talent.Tests.SkillProfile.Policy;

using Acme.Workforce.Talent.SkillProfile.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class SkillProfileAuditChecks
{
    public static void SkillProfileAuditStartsInPlanned()
    {
        var item = SkillProfileAudit.NewSkillProfileAudit("code-741", "title-493");
        Check.Equal(item.Status, SkillProfileAuditStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SkillProfileAuditMovesToCancelled()
    {
        var item = SkillProfileAudit.NewSkillProfileAudit("code-741", "title-493");
        try
        {
            item.MoveTo(SkillProfileAuditStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SkillProfileAuditStatus.Cancelled, "status");
    }

    public static void SkillProfileAuditRejectsMovesFromCancelled()
    {
        var item = SkillProfileAudit.NewSkillProfileAudit("code-741", "title-493");
        item.Status = SkillProfileAuditStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SkillProfileAuditStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SkillProfileAuditStartsInPlanned();
        SkillProfileAuditMovesToCancelled();
        SkillProfileAuditRejectsMovesFromCancelled();
    }
}
