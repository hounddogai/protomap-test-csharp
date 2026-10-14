namespace Acme.Workforce.Contractors.StatementOfWork.Ledger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves statement of work ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record StatementOfWorkLedgerService(IStatementOfWorkLedgerStore Store, StatementOfWorkLedgerValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StatementOfWorkLedgerStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StatementOfWorkLedgerStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of statement of work ledger workflow.</summary>
public static class StatementOfWorkLedgerWorkflow
{
    public static StatementOfWorkLedgerService NewStatementOfWorkLedgerService(IStatementOfWorkLedgerStore store, IClock clock)
    {
        return new StatementOfWorkLedgerService(
            store,
            StatementOfWorkLedgerRules.DefaultStatementOfWorkLedgerValidator(),
            clock,
            new List<string>());
    }
}
