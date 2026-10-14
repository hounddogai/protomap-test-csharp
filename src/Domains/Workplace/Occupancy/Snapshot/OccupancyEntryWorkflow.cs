namespace Acme.Workforce.Workplace.Occupancy.Snapshot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves occupancy entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record OccupancyEntryDesk(IOccupancyEntryStore Store, OccupancyEntryValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(OccupancyEntryStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(OccupancyEntryStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of occupancy entry workflow.</summary>
public static class OccupancyEntryWorkflow
{
    public static OccupancyEntryDesk NewOccupancyEntryDesk(IOccupancyEntryStore store, IClock clock)
    {
        return new OccupancyEntryDesk(
            store,
            OccupancyEntryRules.DefaultOccupancyEntryValidator(),
            clock,
            new List<string>());
    }
}
