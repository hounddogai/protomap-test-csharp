namespace Acme.Workforce.Payrollops.TaxForm.Ledger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves batched tax form ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record BatchedTaxFormLedgerDesk(IBatchedTaxFormLedgerStore Store, BatchedTaxFormLedgerValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched tax form ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedTaxFormLedgerStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched tax form ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedTaxFormLedgerStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public int Audit(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of batched tax form ledger workflow.</summary>
public static class BatchedTaxFormLedgerWorkflow
{
    public static BatchedTaxFormLedgerDesk NewBatchedTaxFormLedgerDesk(IBatchedTaxFormLedgerStore store, IClock clock)
    {
        return new BatchedTaxFormLedgerDesk(
            store,
            BatchedTaxFormLedgerRules.DefaultBatchedTaxFormLedgerValidator(),
            clock,
            new List<string>());
    }
}
