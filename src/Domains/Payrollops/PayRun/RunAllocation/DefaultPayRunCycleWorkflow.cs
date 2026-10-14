namespace Acme.Workforce.Payrollops.PayRun.RunAllocation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default pay run cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultPayRunCycleCoordinator(IDefaultPayRunCycleStore Store, DefaultPayRunCycleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default pay run cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultPayRunCycleStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default pay run cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultPayRunCycleStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of default pay run cycle workflow.</summary>
public static class DefaultPayRunCycleWorkflow
{
    public static DefaultPayRunCycleCoordinator NewDefaultPayRunCycleCoordinator(IDefaultPayRunCycleStore store, IClock clock)
    {
        return new DefaultPayRunCycleCoordinator(
            store,
            DefaultPayRunCycleRules.DefaultDefaultPayRunCycleValidator(),
            clock,
            new List<string>());
    }
}
