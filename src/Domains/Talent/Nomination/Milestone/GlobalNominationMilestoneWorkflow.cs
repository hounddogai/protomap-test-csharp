namespace Acme.Workforce.Talent.Nomination.Milestone;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves global nomination milestones through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record GlobalNominationMilestoneService(IGlobalNominationMilestoneStore Store, GlobalNominationMilestoneValidator Validator, IClock Clock, List<string> Journal, GlobalNominationMilestoneEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global nomination milestone {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalNominationMilestoneStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global nomination milestone {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalNominationMilestoneStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global nomination milestone {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of global nomination milestone workflow.</summary>
public static class GlobalNominationMilestoneWorkflow
{
    public static GlobalNominationMilestoneService NewGlobalNominationMilestoneService(IGlobalNominationMilestoneStore store, IClock clock)
    {
        return new GlobalNominationMilestoneService(
            store,
            GlobalNominationMilestoneRules.DefaultGlobalNominationMilestoneValidator(),
            clock,
            new List<string>(),
            GlobalNominationMilestoneEngine.DefaultGlobalNominationMilestoneEngine());
    }
}
