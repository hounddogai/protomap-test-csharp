namespace Acme.Workforce.Workplace.Booking.Ledger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves batched booking ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record BatchedBookingLedgerManager(IBatchedBookingLedgerStore Store, BatchedBookingLedgerValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched booking ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedBookingLedgerStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched booking ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedBookingLedgerStatus.Cancelled);
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

/// <summary>Functions of batched booking ledger workflow.</summary>
public static class BatchedBookingLedgerWorkflow
{
    public static BatchedBookingLedgerManager NewBatchedBookingLedgerManager(IBatchedBookingLedgerStore store, IClock clock)
    {
        return new BatchedBookingLedgerManager(
            store,
            BatchedBookingLedgerRules.DefaultBatchedBookingLedgerValidator(),
            clock,
            new List<string>());
    }
}
