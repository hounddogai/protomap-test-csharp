namespace Acme.Workforce.Workplace.Booking.BookingAudit;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves booking snapshots through their life cycle and keeps a journal of what it did.</summary>
public sealed record BookingSnapshotService(IBookingSnapshotStore Store, BookingSnapshotValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"booking snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BookingSnapshotStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"booking snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BookingSnapshotStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of booking snapshot workflow.</summary>
public static class BookingSnapshotWorkflow
{
    public static BookingSnapshotService NewBookingSnapshotService(IBookingSnapshotStore store, IClock clock)
    {
        return new BookingSnapshotService(
            store,
            BookingSnapshotRules.DefaultBookingSnapshotValidator(),
            clock,
            new List<string>());
    }
}
