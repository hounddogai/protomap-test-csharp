namespace Acme.Workforce.Absence.Accrual.Digest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered accrual ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredAccrualLedgerCoordinator(ITieredAccrualLedgerStore Store, TieredAccrualLedgerValidator Validator, IClock Clock, List<string> Journal, TieredAccrualLedgerEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered accrual ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredAccrualLedgerStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered accrual ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredAccrualLedgerStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered accrual ledger {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of tiered accrual ledger workflow.</summary>
public static class TieredAccrualLedgerWorkflow
{
    public static TieredAccrualLedgerCoordinator NewTieredAccrualLedgerCoordinator(ITieredAccrualLedgerStore store, IClock clock)
    {
        return new TieredAccrualLedgerCoordinator(
            store,
            TieredAccrualLedgerRules.DefaultTieredAccrualLedgerValidator(),
            clock,
            new List<string>(),
            TieredAccrualLedgerEngine.DefaultTieredAccrualLedgerEngine());
    }
}
