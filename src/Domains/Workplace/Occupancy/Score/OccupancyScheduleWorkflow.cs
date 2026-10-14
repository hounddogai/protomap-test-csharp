namespace Acme.Workforce.Workplace.Occupancy.Score;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves occupancy schedules through their life cycle and keeps a journal of what it did.</summary>
public sealed record OccupancyScheduleCoordinator(IOccupancyScheduleStore Store, OccupancyScheduleValidator Validator, IClock Clock, List<string> Journal, OccupancyScheduleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(OccupancyScheduleStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(OccupancyScheduleStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy schedule {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of occupancy schedule workflow.</summary>
public static class OccupancyScheduleWorkflow
{
    public static OccupancyScheduleCoordinator NewOccupancyScheduleCoordinator(IOccupancyScheduleStore store, IClock clock)
    {
        return new OccupancyScheduleCoordinator(
            store,
            OccupancyScheduleRules.DefaultOccupancyScheduleValidator(),
            clock,
            new List<string>(),
            OccupancyScheduleEngine.DefaultOccupancyScheduleEngine());
    }
}
