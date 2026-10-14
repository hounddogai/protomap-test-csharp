namespace Acme.Workforce.Workplace.Booking.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves booking digests through their life cycle and keeps a journal of what it did.</summary>
public sealed record BookingDigestManager(IBookingDigestStore Store, BookingDigestValidator Validator, IClock Clock, List<string> Journal, BookingDigestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"booking digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BookingDigestStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"booking digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BookingDigestStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"booking digest {code} not found");
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

/// <summary>Functions of booking digest workflow.</summary>
public static class BookingDigestWorkflow
{
    public static BookingDigestManager NewBookingDigestManager(IBookingDigestStore store, IClock clock, Registry flags)
    {
        return new BookingDigestManager(
            store,
            BookingDigestRules.DefaultBookingDigestValidator(),
            clock,
            new List<string>(),
            BookingDigestEngine.DefaultBookingDigestEngine(flags));
    }
}
