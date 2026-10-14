namespace Acme.Workforce.Workplace.Booking.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves booking preferences through their life cycle and keeps a journal of what it did.</summary>
public sealed record BookingPreferenceService(IBookingPreferenceStore Store, BookingPreferenceValidator Validator, IClock Clock, List<string> Journal, BookingPreferenceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"booking preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BookingPreferenceStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"booking preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BookingPreferenceStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"booking preference {code} not found");
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

/// <summary>Functions of booking preference workflow.</summary>
public static class BookingPreferenceWorkflow
{
    public static BookingPreferenceService NewBookingPreferenceService(IBookingPreferenceStore store, IClock clock, Registry flags)
    {
        return new BookingPreferenceService(
            store,
            BookingPreferenceRules.DefaultBookingPreferenceValidator(),
            clock,
            new List<string>(),
            BookingPreferenceEngine.DefaultBookingPreferenceEngine(flags));
    }
}
