namespace Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal accrual ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record SeasonalAccrualLedgerService(ISeasonalAccrualLedgerStore Store, SeasonalAccrualLedgerValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal accrual ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalAccrualLedgerStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal accrual ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalAccrualLedgerStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of seasonal accrual ledger workflow.</summary>
public static class SeasonalAccrualLedgerWorkflow
{
    public static SeasonalAccrualLedgerService NewSeasonalAccrualLedgerService(ISeasonalAccrualLedgerStore store, IClock clock)
    {
        return new SeasonalAccrualLedgerService(
            store,
            SeasonalAccrualLedgerRules.DefaultSeasonalAccrualLedgerValidator(),
            clock,
            new List<string>());
    }
}
