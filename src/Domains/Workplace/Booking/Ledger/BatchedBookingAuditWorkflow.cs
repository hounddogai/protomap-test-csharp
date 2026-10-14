namespace Acme.Workforce.Workplace.Booking.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves batched booking audits through their life cycle and keeps a journal of what it did.</summary>
public sealed record BatchedBookingAuditWorkflow(IBatchedBookingAuditStore Store, BatchedBookingAuditValidator Validator, IClock Clock, List<string> Journal, BatchedBookingAuditEngine Engine)
{
    public static BatchedBookingAuditWorkflow NewBatchedBookingAuditWorkflow(IBatchedBookingAuditStore store, IClock clock, Registry flags)
    {
        return new BatchedBookingAuditWorkflow(
            store,
            BatchedBookingAuditRules.DefaultBatchedBookingAuditValidator(),
            clock,
            new List<string>(),
            BatchedBookingAuditEngine.DefaultBatchedBookingAuditEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched booking audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedBookingAuditStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched booking audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedBookingAuditStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched booking audit {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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
