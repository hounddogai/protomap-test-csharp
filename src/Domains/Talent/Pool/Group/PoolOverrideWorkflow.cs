namespace Acme.Workforce.Talent.Pool.Group;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pool overrides through their life cycle and keeps a journal of what it did.</summary>
public sealed record PoolOverrideService(IPoolOverrideStore Store, PoolOverrideValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PoolOverrideStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PoolOverrideStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of pool override workflow.</summary>
public static class PoolOverrideWorkflow
{
    public static PoolOverrideService NewPoolOverrideService(IPoolOverrideStore store, IClock clock)
    {
        return new PoolOverrideService(
            store,
            PoolOverrideRules.DefaultPoolOverrideValidator(),
            clock,
            new List<string>());
    }
}
