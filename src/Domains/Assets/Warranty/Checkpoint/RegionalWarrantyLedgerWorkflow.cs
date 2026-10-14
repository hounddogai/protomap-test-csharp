namespace Acme.Workforce.Assets.Warranty.Checkpoint;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional warranty ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record RegionalWarrantyLedgerCoordinator(IRegionalWarrantyLedgerStore Store, RegionalWarrantyLedgerValidator Validator, IClock Clock, List<string> Journal, RegionalWarrantyLedgerEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional warranty ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalWarrantyLedgerStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional warranty ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalWarrantyLedgerStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional warranty ledger {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of regional warranty ledger workflow.</summary>
public static class RegionalWarrantyLedgerWorkflow
{
    public static RegionalWarrantyLedgerCoordinator NewRegionalWarrantyLedgerCoordinator(IRegionalWarrantyLedgerStore store, IClock clock)
    {
        return new RegionalWarrantyLedgerCoordinator(
            store,
            RegionalWarrantyLedgerRules.DefaultRegionalWarrantyLedgerValidator(),
            clock,
            new List<string>(),
            RegionalWarrantyLedgerEngine.DefaultRegionalWarrantyLedgerEngine());
    }
}
