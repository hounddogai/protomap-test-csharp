namespace Acme.Workforce.Talent.Pool.Threshold;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pool targets through their life cycle and keeps a journal of what it did.</summary>
public sealed record PoolTargetDesk(IPoolTargetStore Store, PoolTargetValidator Validator, IClock Clock, List<string> Journal, PoolTargetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PoolTargetStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PoolTargetStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool target {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of pool target workflow.</summary>
public static class PoolTargetWorkflow
{
    public static PoolTargetDesk NewPoolTargetDesk(IPoolTargetStore store, IClock clock)
    {
        return new PoolTargetDesk(
            store,
            PoolTargetRules.DefaultPoolTargetValidator(),
            clock,
            new List<string>(),
            PoolTargetEngine.DefaultPoolTargetEngine());
    }
}
