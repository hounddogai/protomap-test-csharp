namespace Acme.Workforce.Absence.Accrual.Ledger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves accrual ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccrualLedgerDesk(IAccrualLedgerStore Store, AccrualLedgerValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccrualLedgerStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccrualLedgerStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of accrual ledger workflow.</summary>
public static class AccrualLedgerWorkflow
{
    public static AccrualLedgerDesk NewAccrualLedgerDesk(IAccrualLedgerStore store, IClock clock)
    {
        return new AccrualLedgerDesk(
            store,
            AccrualLedgerRules.DefaultAccrualLedgerValidator(),
            clock,
            new List<string>());
    }
}
