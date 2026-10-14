namespace Acme.Workforce.Workplace.Booking.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves booking requests through their life cycle and keeps a journal of what it did.</summary>
public sealed record BookingRequestCoordinator(IBookingRequestStore Store, BookingRequestValidator Validator, IClock Clock, List<string> Journal, BookingRequestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"booking request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BookingRequestStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"booking request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BookingRequestStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"booking request {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int ReviewStale(int cutoff)
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

/// <summary>Functions of booking request workflow.</summary>
public static class BookingRequestWorkflow
{
    public static BookingRequestCoordinator NewBookingRequestCoordinator(IBookingRequestStore store, IClock clock, Registry flags)
    {
        return new BookingRequestCoordinator(
            store,
            BookingRequestRules.DefaultBookingRequestValidator(),
            clock,
            new List<string>(),
            BookingRequestEngine.DefaultBookingRequestEngine(flags));
    }
}
