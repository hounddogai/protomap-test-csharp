namespace Acme.Workforce.Workplace.Booking.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves strict booking thresholds through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictBookingThresholdDesk(IStrictBookingThresholdStore Store, StrictBookingThresholdValidator Validator, IClock Clock, List<string> Journal, StrictBookingThresholdEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict booking threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictBookingThresholdStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict booking threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictBookingThresholdStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict booking threshold {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of strict booking threshold workflow.</summary>
public static class StrictBookingThresholdWorkflow
{
    public static StrictBookingThresholdDesk NewStrictBookingThresholdDesk(IStrictBookingThresholdStore store, IClock clock, Registry flags)
    {
        return new StrictBookingThresholdDesk(
            store,
            StrictBookingThresholdRules.DefaultStrictBookingThresholdValidator(),
            clock,
            new List<string>(),
            StrictBookingThresholdEngine.DefaultStrictBookingThresholdEngine(flags));
    }
}
