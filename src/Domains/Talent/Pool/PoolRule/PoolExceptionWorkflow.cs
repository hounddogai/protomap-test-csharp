namespace Acme.Workforce.Talent.Pool.PoolRule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pool exceptions through their life cycle and keeps a journal of what it did.</summary>
public sealed record PoolExceptionDesk(IPoolExceptionStore Store, PoolExceptionValidator Validator, IClock Clock, List<string> Journal, PoolExceptionEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PoolExceptionStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PoolExceptionStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool exception {code} not found");
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

/// <summary>Functions of pool exception workflow.</summary>
public static class PoolExceptionWorkflow
{
    public static PoolExceptionDesk NewPoolExceptionDesk(IPoolExceptionStore store, IClock clock, Registry flags)
    {
        return new PoolExceptionDesk(
            store,
            PoolExceptionRules.DefaultPoolExceptionValidator(),
            clock,
            new List<string>(),
            PoolExceptionEngine.DefaultPoolExceptionEngine(flags));
    }
}
