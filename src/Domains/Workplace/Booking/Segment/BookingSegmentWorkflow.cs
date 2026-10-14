namespace Acme.Workforce.Workplace.Booking.Segment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves booking segments through their life cycle and keeps a journal of what it did.</summary>
public sealed record BookingSegmentService(IBookingSegmentStore Store, BookingSegmentValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"booking segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BookingSegmentStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"booking segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BookingSegmentStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of booking segment workflow.</summary>
public static class BookingSegmentWorkflow
{
    public static BookingSegmentService NewBookingSegmentService(IBookingSegmentStore store, IClock clock)
    {
        return new BookingSegmentService(
            store,
            BookingSegmentRules.DefaultBookingSegmentValidator(),
            clock,
            new List<string>());
    }
}
