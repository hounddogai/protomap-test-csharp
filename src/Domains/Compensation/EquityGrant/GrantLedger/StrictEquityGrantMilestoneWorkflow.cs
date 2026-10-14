namespace Acme.Workforce.Compensation.EquityGrant.GrantLedger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves strict equity grant milestones through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record StrictEquityGrantMilestoneManager(IStrictEquityGrantMilestoneStore Store, StrictEquityGrantMilestoneValidator Validator, IClock Clock, List<string> Journal, StrictEquityGrantMilestoneEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict equity grant milestone {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictEquityGrantMilestoneStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict equity grant milestone {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictEquityGrantMilestoneStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict equity grant milestone {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Sweep(int cutoff)
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

/// <summary>Functions of strict equity grant milestone workflow.</summary>
public static class StrictEquityGrantMilestoneWorkflow
{
    public static StrictEquityGrantMilestoneManager NewStrictEquityGrantMilestoneManager(IStrictEquityGrantMilestoneStore store, IClock clock, Registry flags)
    {
        return new StrictEquityGrantMilestoneManager(
            store,
            StrictEquityGrantMilestoneRules.DefaultStrictEquityGrantMilestoneValidator(),
            clock,
            new List<string>(),
            StrictEquityGrantMilestoneEngine.DefaultStrictEquityGrantMilestoneEngine(flags));
    }
}
