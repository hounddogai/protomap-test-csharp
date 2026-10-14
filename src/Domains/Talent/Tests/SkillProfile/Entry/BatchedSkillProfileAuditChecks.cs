namespace Acme.Workforce.Talent.Tests.SkillProfile.Entry;

using Acme.Workforce.Talent.SkillProfile.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class BatchedSkillProfileAuditChecks
{
    public static void BatchedSkillProfileAuditStartsInPlanned()
    {
        var item = BatchedSkillProfileAudit.NewBatchedSkillProfileAudit("code-718", "name-408");
        Check.Equal(item.Status, BatchedSkillProfileAuditStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedSkillProfileAuditMovesToInProgress()
    {
        var item = BatchedSkillProfileAudit.NewBatchedSkillProfileAudit("code-718", "name-408");
        try
        {
            item.MoveTo(BatchedSkillProfileAuditStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedSkillProfileAuditStatus.InProgress, "status");
    }

    public static void BatchedSkillProfileAuditRejectsMovesFromCancelled()
    {
        var item = BatchedSkillProfileAudit.NewBatchedSkillProfileAudit("code-718", "name-408");
        item.Status = BatchedSkillProfileAuditStatus.Cancelled;
        Check.Fails(() => item.MoveTo(BatchedSkillProfileAuditStatus.Planned), "move");
    }

    public static void RunAll()
    {
        BatchedSkillProfileAuditStartsInPlanned();
        BatchedSkillProfileAuditMovesToInProgress();
        BatchedSkillProfileAuditRejectsMovesFromCancelled();
    }
}
