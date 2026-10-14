namespace Acme.Workforce.Talent.Pool.Audit;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pool audits through their life cycle and keeps a journal of what it did.</summary>
public sealed record PoolAuditDesk(IPoolAuditStore Store, PoolAuditValidator Validator, IClock Clock, List<string> Journal, PoolAuditEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PoolAuditStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PoolAuditStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool audit {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of pool audit workflow.</summary>
public static class PoolAuditWorkflow
{
    public static PoolAuditDesk NewPoolAuditDesk(IPoolAuditStore store, IClock clock)
    {
        return new PoolAuditDesk(
            store,
            PoolAuditRules.DefaultPoolAuditValidator(),
            clock,
            new List<string>(),
            PoolAuditEngine.DefaultPoolAuditEngine());
    }
}
