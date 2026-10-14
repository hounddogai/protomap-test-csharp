namespace Acme.Workforce.Workplace.Occupancy.Review;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves occupancy snapshots through their life cycle and keeps a journal of what it did.</summary>
public sealed record OccupancySnapshotCoordinator(IOccupancySnapshotStore Store, OccupancySnapshotValidator Validator, IClock Clock, List<string> Journal, OccupancySnapshotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(OccupancySnapshotStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(OccupancySnapshotStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy snapshot {code} not found");
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

/// <summary>Functions of occupancy snapshot workflow.</summary>
public static class OccupancySnapshotWorkflow
{
    public static OccupancySnapshotCoordinator NewOccupancySnapshotCoordinator(IOccupancySnapshotStore store, IClock clock)
    {
        return new OccupancySnapshotCoordinator(
            store,
            OccupancySnapshotRules.DefaultOccupancySnapshotValidator(),
            clock,
            new List<string>(),
            OccupancySnapshotEngine.DefaultOccupancySnapshotEngine());
    }
}
