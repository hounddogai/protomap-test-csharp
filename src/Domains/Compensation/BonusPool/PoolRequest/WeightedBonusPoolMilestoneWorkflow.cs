namespace Acme.Workforce.Compensation.BonusPool.PoolRequest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted bonus pool milestones through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record WeightedBonusPoolMilestoneService(IWeightedBonusPoolMilestoneStore Store, WeightedBonusPoolMilestoneValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted bonus pool milestone {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedBonusPoolMilestoneStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted bonus pool milestone {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedBonusPoolMilestoneStatus.Cancelled);
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

/// <summary>Functions of weighted bonus pool milestone workflow.</summary>
public static class WeightedBonusPoolMilestoneWorkflow
{
    public static WeightedBonusPoolMilestoneService NewWeightedBonusPoolMilestoneService(IWeightedBonusPoolMilestoneStore store, IClock clock)
    {
        return new WeightedBonusPoolMilestoneService(
            store,
            WeightedBonusPoolMilestoneRules.DefaultWeightedBonusPoolMilestoneValidator(),
            clock,
            new List<string>());
    }
}
