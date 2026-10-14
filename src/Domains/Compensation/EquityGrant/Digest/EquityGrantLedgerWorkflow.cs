namespace Acme.Workforce.Compensation.EquityGrant.Digest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves equity grant ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record EquityGrantLedgerCoordinator(IEquityGrantLedgerStore Store, EquityGrantLedgerValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(EquityGrantLedgerStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(EquityGrantLedgerStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of equity grant ledger workflow.</summary>
public static class EquityGrantLedgerWorkflow
{
    public static EquityGrantLedgerCoordinator NewEquityGrantLedgerCoordinator(IEquityGrantLedgerStore store, IClock clock)
    {
        return new EquityGrantLedgerCoordinator(
            store,
            EquityGrantLedgerRules.DefaultEquityGrantLedgerValidator(),
            clock,
            new List<string>());
    }
}
