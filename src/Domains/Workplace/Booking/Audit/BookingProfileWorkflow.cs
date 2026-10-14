namespace Acme.Workforce.Workplace.Booking.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves booking profiles through their life cycle and keeps a journal of what it did.</summary>
public sealed record BookingProfileDesk(IBookingProfileStore Store, BookingProfileValidator Validator, IClock Clock, List<string> Journal, BookingProfileEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"booking profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BookingProfileStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"booking profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BookingProfileStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"booking profile {code} not found");
        }
        var result = Engine.Price(item);
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of booking profile workflow.</summary>
public static class BookingProfileWorkflow
{
    public static BookingProfileDesk NewBookingProfileDesk(IBookingProfileStore store, IClock clock, Registry flags)
    {
        return new BookingProfileDesk(
            store,
            BookingProfileRules.DefaultBookingProfileValidator(),
            clock,
            new List<string>(),
            BookingProfileEngine.DefaultBookingProfileEngine(flags));
    }
}
