namespace Acme.Workforce.Assets.Laptop.Slot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending laptop ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record PendingLaptopLedgerDesk(IPendingLaptopLedgerStore Store, PendingLaptopLedgerValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending laptop ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingLaptopLedgerStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending laptop ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingLaptopLedgerStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of pending laptop ledger workflow.</summary>
public static class PendingLaptopLedgerWorkflow
{
    public static PendingLaptopLedgerDesk NewPendingLaptopLedgerDesk(IPendingLaptopLedgerStore store, IClock clock)
    {
        return new PendingLaptopLedgerDesk(
            store,
            PendingLaptopLedgerRules.DefaultPendingLaptopLedgerValidator(),
            clock,
            new List<string>());
    }
}
