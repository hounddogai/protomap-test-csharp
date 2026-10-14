namespace Acme.Workforce.Contractors.Rate.RateBatch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves rate milestones through their life cycle and keeps a journal of what it did.</summary>
public sealed record RateMilestoneCoordinator(IRateMilestoneStore Store, RateMilestoneValidator Validator, IClock Clock, List<string> Journal, RateMilestoneEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rate milestone {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RateMilestoneStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rate milestone {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RateMilestoneStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rate milestone {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Scan(int cutoff)
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

/// <summary>Functions of rate milestone workflow.</summary>
public static class RateMilestoneWorkflow
{
    public static RateMilestoneCoordinator NewRateMilestoneCoordinator(IRateMilestoneStore store, IClock clock, Registry flags)
    {
        return new RateMilestoneCoordinator(
            store,
            RateMilestoneRules.DefaultRateMilestoneValidator(),
            clock,
            new List<string>(),
            RateMilestoneEngine.DefaultRateMilestoneEngine(flags));
    }
}
