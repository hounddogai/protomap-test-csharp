namespace Acme.Workforce.Assets.Laptop.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves laptop ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record LaptopLedgerCoordinator(ILaptopLedgerStore Store, LaptopLedgerValidator Validator, IClock Clock, List<string> Journal, LaptopLedgerEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"laptop ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LaptopLedgerStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"laptop ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LaptopLedgerStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"laptop ledger {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of laptop ledger workflow.</summary>
public static class LaptopLedgerWorkflow
{
    public static LaptopLedgerCoordinator NewLaptopLedgerCoordinator(ILaptopLedgerStore store, IClock clock, Registry flags)
    {
        return new LaptopLedgerCoordinator(
            store,
            LaptopLedgerRules.DefaultLaptopLedgerValidator(),
            clock,
            new List<string>(),
            LaptopLedgerEngine.DefaultLaptopLedgerEngine(flags));
    }
}
