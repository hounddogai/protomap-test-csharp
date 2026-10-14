namespace Acme.Workforce.Engagement.TownHall.Ledger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict town hall ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictTownHallLedgerManager(IStrictTownHallLedgerStore Store, StrictTownHallLedgerValidator Validator, IClock Clock, List<string> Journal, StrictTownHallLedgerEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict town hall ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictTownHallLedgerStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict town hall ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictTownHallLedgerStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict town hall ledger {code} not found");
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

/// <summary>Functions of strict town hall ledger workflow.</summary>
public static class StrictTownHallLedgerWorkflow
{
    public static StrictTownHallLedgerManager NewStrictTownHallLedgerManager(IStrictTownHallLedgerStore store, IClock clock)
    {
        return new StrictTownHallLedgerManager(
            store,
            StrictTownHallLedgerRules.DefaultStrictTownHallLedgerValidator(),
            clock,
            new List<string>(),
            StrictTownHallLedgerEngine.DefaultStrictTownHallLedgerEngine());
    }
}
