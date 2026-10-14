namespace Acme.Workforce.Workplace.Booking.Plan;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves booking metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record BookingMetricCoordinator(IBookingMetricStore Store, BookingMetricValidator Validator, IClock Clock, List<string> Journal, BookingMetricEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"booking metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BookingMetricStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"booking metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BookingMetricStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"booking metric {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of booking metric workflow.</summary>
public static class BookingMetricWorkflow
{
    public static BookingMetricCoordinator NewBookingMetricCoordinator(IBookingMetricStore store, IClock clock)
    {
        return new BookingMetricCoordinator(
            store,
            BookingMetricRules.DefaultBookingMetricValidator(),
            clock,
            new List<string>(),
            BookingMetricEngine.DefaultBookingMetricEngine());
    }
}
