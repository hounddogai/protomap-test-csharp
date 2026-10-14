namespace Acme.Workforce.Talent.Pool.Request;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pool agreements through their life cycle and keeps a journal of what it did.</summary>
public sealed record PoolAgreementCoordinator(IPoolAgreementStore Store, PoolAgreementValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool agreement {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PoolAgreementStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool agreement {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PoolAgreementStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of pool agreement workflow.</summary>
public static class PoolAgreementWorkflow
{
    public static PoolAgreementCoordinator NewPoolAgreementCoordinator(IPoolAgreementStore store, IClock clock)
    {
        return new PoolAgreementCoordinator(
            store,
            PoolAgreementRules.DefaultPoolAgreementValidator(),
            clock,
            new List<string>());
    }
}
