namespace Acme.Workforce.Workplace.Booking.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves booking checkpoints through their life cycle and keeps a journal of what it did.</summary>
public sealed record BookingCheckpointDesk(IBookingCheckpointStore Store, BookingCheckpointValidator Validator, IClock Clock, List<string> Journal, BookingCheckpointEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"booking checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BookingCheckpointStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"booking checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BookingCheckpointStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"booking checkpoint {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of booking checkpoint workflow.</summary>
public static class BookingCheckpointWorkflow
{
    public static BookingCheckpointDesk NewBookingCheckpointDesk(IBookingCheckpointStore store, IClock clock, Registry flags)
    {
        return new BookingCheckpointDesk(
            store,
            BookingCheckpointRules.DefaultBookingCheckpointValidator(),
            clock,
            new List<string>(),
            BookingCheckpointEngine.DefaultBookingCheckpointEngine(flags));
    }
}
