namespace Acme.Workforce.Talent.Pool.Group;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves rolling pool ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record RollingPoolLedgerCoordinator(IRollingPoolLedgerStore Store, RollingPoolLedgerValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling pool ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingPoolLedgerStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling pool ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingPoolLedgerStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of rolling pool ledger workflow.</summary>
public static class RollingPoolLedgerWorkflow
{
    public static RollingPoolLedgerCoordinator NewRollingPoolLedgerCoordinator(IRollingPoolLedgerStore store, IClock clock)
    {
        return new RollingPoolLedgerCoordinator(
            store,
            RollingPoolLedgerRules.DefaultRollingPoolLedgerValidator(),
            clock,
            new List<string>());
    }
}
