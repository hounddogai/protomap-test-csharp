namespace Acme.Workforce.Workplace.Booking.BookingAudit;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled booking audits through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScheduledBookingAuditCoordinator(IScheduledBookingAuditStore Store, ScheduledBookingAuditValidator Validator, IClock Clock, List<string> Journal, ScheduledBookingAuditEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled booking audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledBookingAuditStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled booking audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledBookingAuditStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled booking audit {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of scheduled booking audit workflow.</summary>
public static class ScheduledBookingAuditWorkflow
{
    public static ScheduledBookingAuditCoordinator NewScheduledBookingAuditCoordinator(IScheduledBookingAuditStore store, IClock clock)
    {
        return new ScheduledBookingAuditCoordinator(
            store,
            ScheduledBookingAuditRules.DefaultScheduledBookingAuditValidator(),
            clock,
            new List<string>(),
            ScheduledBookingAuditEngine.DefaultScheduledBookingAuditEngine());
    }
}
