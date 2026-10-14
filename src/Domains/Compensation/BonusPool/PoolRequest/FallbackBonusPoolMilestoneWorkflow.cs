namespace Acme.Workforce.Compensation.BonusPool.PoolRequest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback bonus pool milestones through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackBonusPoolMilestoneService(IFallbackBonusPoolMilestoneStore Store, FallbackBonusPoolMilestoneValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback bonus pool milestone {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackBonusPoolMilestoneStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback bonus pool milestone {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackBonusPoolMilestoneStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of fallback bonus pool milestone workflow.</summary>
public static class FallbackBonusPoolMilestoneWorkflow
{
    public static FallbackBonusPoolMilestoneService NewFallbackBonusPoolMilestoneService(IFallbackBonusPoolMilestoneStore store, IClock clock)
    {
        return new FallbackBonusPoolMilestoneService(
            store,
            FallbackBonusPoolMilestoneRules.DefaultFallbackBonusPoolMilestoneValidator(),
            clock,
            new List<string>());
    }
}
