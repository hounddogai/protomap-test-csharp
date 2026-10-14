namespace Acme.Workforce.Workplace.Occupancy.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves occupancy groups through their life cycle and keeps a journal of what it did.</summary>
public sealed record OccupancyGroupCoordinator(IOccupancyGroupStore Store, OccupancyGroupValidator Validator, IClock Clock, List<string> Journal, OccupancyGroupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy group {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(OccupancyGroupStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy group {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(OccupancyGroupStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy group {code} not found");
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of occupancy group workflow.</summary>
public static class OccupancyGroupWorkflow
{
    public static OccupancyGroupCoordinator NewOccupancyGroupCoordinator(IOccupancyGroupStore store, IClock clock, Registry flags)
    {
        return new OccupancyGroupCoordinator(
            store,
            OccupancyGroupRules.DefaultOccupancyGroupValidator(),
            clock,
            new List<string>(),
            OccupancyGroupEngine.DefaultOccupancyGroupEngine(flags));
    }
}
