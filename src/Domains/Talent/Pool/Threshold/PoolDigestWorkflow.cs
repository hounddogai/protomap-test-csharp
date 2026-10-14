namespace Acme.Workforce.Talent.Pool.Threshold;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pool digests through their life cycle and keeps a journal of what it did.</summary>
public sealed record PoolDigestService(IPoolDigestStore Store, PoolDigestValidator Validator, IClock Clock, List<string> Journal, PoolDigestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PoolDigestStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PoolDigestStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool digest {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of pool digest workflow.</summary>
public static class PoolDigestWorkflow
{
    public static PoolDigestService NewPoolDigestService(IPoolDigestStore store, IClock clock)
    {
        return new PoolDigestService(
            store,
            PoolDigestRules.DefaultPoolDigestValidator(),
            clock,
            new List<string>(),
            PoolDigestEngine.DefaultPoolDigestEngine());
    }
}
