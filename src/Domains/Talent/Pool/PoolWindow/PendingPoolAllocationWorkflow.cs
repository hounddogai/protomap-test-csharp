namespace Acme.Workforce.Talent.Pool.PoolWindow;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending pool allocations through their life cycle and keeps a journal of what it did.</summary>
public sealed record PendingPoolAllocationCoordinator(IPendingPoolAllocationStore Store, PendingPoolAllocationValidator Validator, IClock Clock, List<string> Journal, PendingPoolAllocationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending pool allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingPoolAllocationStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending pool allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingPoolAllocationStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending pool allocation {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of pending pool allocation workflow.</summary>
public static class PendingPoolAllocationWorkflow
{
    public static PendingPoolAllocationCoordinator NewPendingPoolAllocationCoordinator(IPendingPoolAllocationStore store, IClock clock)
    {
        return new PendingPoolAllocationCoordinator(
            store,
            PendingPoolAllocationRules.DefaultPendingPoolAllocationValidator(),
            clock,
            new List<string>(),
            PendingPoolAllocationEngine.DefaultPendingPoolAllocationEngine());
    }
}
