namespace Acme.Workforce.Engagement.TownHall.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves adaptive town hall ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdaptiveTownHallLedgerCoordinator(IAdaptiveTownHallLedgerStore Store, AdaptiveTownHallLedgerValidator Validator, IClock Clock, List<string> Journal, AdaptiveTownHallLedgerEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive town hall ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveTownHallLedgerStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive town hall ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveTownHallLedgerStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive town hall ledger {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of adaptive town hall ledger workflow.</summary>
public static class AdaptiveTownHallLedgerWorkflow
{
    public static AdaptiveTownHallLedgerCoordinator NewAdaptiveTownHallLedgerCoordinator(IAdaptiveTownHallLedgerStore store, IClock clock, Registry flags)
    {
        return new AdaptiveTownHallLedgerCoordinator(
            store,
            AdaptiveTownHallLedgerRules.DefaultAdaptiveTownHallLedgerValidator(),
            clock,
            new List<string>(),
            AdaptiveTownHallLedgerEngine.DefaultAdaptiveTownHallLedgerEngine(flags));
    }
}
