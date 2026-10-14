namespace Acme.Workforce.Workplace.Booking.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves primary booking metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryBookingMetricCoordinator(IPrimaryBookingMetricStore Store, PrimaryBookingMetricValidator Validator, IClock Clock, List<string> Journal, PrimaryBookingMetricEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary booking metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryBookingMetricStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary booking metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryBookingMetricStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary booking metric {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Sweep(int cutoff)
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

/// <summary>Functions of primary booking metric workflow.</summary>
public static class PrimaryBookingMetricWorkflow
{
    public static PrimaryBookingMetricCoordinator NewPrimaryBookingMetricCoordinator(IPrimaryBookingMetricStore store, IClock clock, Registry flags)
    {
        return new PrimaryBookingMetricCoordinator(
            store,
            PrimaryBookingMetricRules.DefaultPrimaryBookingMetricValidator(),
            clock,
            new List<string>(),
            PrimaryBookingMetricEngine.DefaultPrimaryBookingMetricEngine(flags));
    }
}
