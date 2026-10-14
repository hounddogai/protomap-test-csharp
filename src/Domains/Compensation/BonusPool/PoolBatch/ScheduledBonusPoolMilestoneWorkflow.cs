namespace Acme.Workforce.Compensation.BonusPool.PoolBatch;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled bonus pool milestones through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledBonusPoolMilestoneDesk(IScheduledBonusPoolMilestoneStore Store, ScheduledBonusPoolMilestoneValidator Validator, IClock Clock, List<string> Journal, ScheduledBonusPoolMilestoneEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled bonus pool milestone {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledBonusPoolMilestoneStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled bonus pool milestone {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledBonusPoolMilestoneStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled bonus pool milestone {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int ReviewStale(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of scheduled bonus pool milestone workflow.</summary>
public static class ScheduledBonusPoolMilestoneWorkflow
{
    public static ScheduledBonusPoolMilestoneDesk NewScheduledBonusPoolMilestoneDesk(IScheduledBonusPoolMilestoneStore store, IClock clock)
    {
        return new ScheduledBonusPoolMilestoneDesk(
            store,
            ScheduledBonusPoolMilestoneRules.DefaultScheduledBonusPoolMilestoneValidator(),
            clock,
            new List<string>(),
            ScheduledBonusPoolMilestoneEngine.DefaultScheduledBonusPoolMilestoneEngine());
    }
}
