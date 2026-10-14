namespace Acme.Workforce.Talent.SkillProfile.Entry;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves batched skill profile audits through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record BatchedSkillProfileAuditService(IBatchedSkillProfileAuditStore Store, BatchedSkillProfileAuditValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched skill profile audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedSkillProfileAuditStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched skill profile audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedSkillProfileAuditStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public int Audit(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.EndsAt > cutoff)
            {
                continue;
            }
            var problems = Validator.Validate(item);
            if (problems.Count == 0)
            {
                continue;
            }
            Note($"{item.Code} needs attention: {string.Join(", ", problems)}");
            changed += 1;
        }
        return changed;
    }
}

/// <summary>Functions of batched skill profile audit workflow.</summary>
public static class BatchedSkillProfileAuditWorkflow
{
    public static BatchedSkillProfileAuditService NewBatchedSkillProfileAuditService(IBatchedSkillProfileAuditStore store, IClock clock)
    {
        return new BatchedSkillProfileAuditService(
            store,
            BatchedSkillProfileAuditRules.DefaultBatchedSkillProfileAuditValidator(),
            clock,
            new List<string>());
    }
}
