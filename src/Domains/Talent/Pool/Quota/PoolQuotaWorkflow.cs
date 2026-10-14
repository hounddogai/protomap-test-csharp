namespace Acme.Workforce.Talent.Pool.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pool quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record PoolQuotaDesk(IPoolQuotaStore Store, PoolQuotaValidator Validator, IClock Clock, List<string> Journal, PoolQuotaEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PoolQuotaStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PoolQuotaStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool quota {code} not found");
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

/// <summary>Functions of pool quota workflow.</summary>
public static class PoolQuotaWorkflow
{
    public static PoolQuotaDesk NewPoolQuotaDesk(IPoolQuotaStore store, IClock clock, Registry flags)
    {
        return new PoolQuotaDesk(
            store,
            PoolQuotaRules.DefaultPoolQuotaValidator(),
            clock,
            new List<string>(),
            PoolQuotaEngine.DefaultPoolQuotaEngine(flags));
    }
}
