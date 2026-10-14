namespace Acme.Workforce.Workplace.Booking.BookingAudit;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict booking audits through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictBookingAuditManager(IStrictBookingAuditStore Store, StrictBookingAuditValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict booking audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictBookingAuditStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict booking audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictBookingAuditStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of strict booking audit workflow.</summary>
public static class StrictBookingAuditWorkflow
{
    public static StrictBookingAuditManager NewStrictBookingAuditManager(IStrictBookingAuditStore store, IClock clock)
    {
        return new StrictBookingAuditManager(
            store,
            StrictBookingAuditRules.DefaultStrictBookingAuditValidator(),
            clock,
            new List<string>());
    }
}
