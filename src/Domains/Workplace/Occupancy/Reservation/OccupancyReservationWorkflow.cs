namespace Acme.Workforce.Workplace.Occupancy.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves occupancy reservations through their life cycle and keeps a journal of what it did.</summary>
public sealed record OccupancyReservationWorkflow(IOccupancyReservationStore Store, OccupancyReservationValidator Validator, IClock Clock, List<string> Journal, OccupancyReservationEngine Engine)
{
    public static OccupancyReservationWorkflow NewOccupancyReservationWorkflow(IOccupancyReservationStore store, IClock clock, Registry flags)
    {
        return new OccupancyReservationWorkflow(
            store,
            OccupancyReservationRules.DefaultOccupancyReservationValidator(),
            clock,
            new List<string>(),
            OccupancyReservationEngine.DefaultOccupancyReservationEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(OccupancyReservationStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(OccupancyReservationStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy reservation {code} not found");
        }
        var result = Engine.Compute(item);
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
