namespace Acme.Workforce.Workplace.Booking.Baseline;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves booking adjustments through their life cycle and keeps a journal of what it did.</summary>
public sealed record BookingAdjustmentCoordinator(IBookingAdjustmentStore Store, BookingAdjustmentValidator Validator, IClock Clock, List<string> Journal, BookingAdjustmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"booking adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BookingAdjustmentStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"booking adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BookingAdjustmentStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"booking adjustment {code} not found");
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

/// <summary>Functions of booking adjustment workflow.</summary>
public static class BookingAdjustmentWorkflow
{
    public static BookingAdjustmentCoordinator NewBookingAdjustmentCoordinator(IBookingAdjustmentStore store, IClock clock)
    {
        return new BookingAdjustmentCoordinator(
            store,
            BookingAdjustmentRules.DefaultBookingAdjustmentValidator(),
            clock,
            new List<string>(),
            BookingAdjustmentEngine.DefaultBookingAdjustmentEngine());
    }
}
