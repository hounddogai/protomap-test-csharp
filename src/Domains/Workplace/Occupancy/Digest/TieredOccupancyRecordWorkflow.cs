namespace Acme.Workforce.Workplace.Occupancy.Digest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered occupancy records through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredOccupancyRecordManager(ITieredOccupancyRecordStore Store, TieredOccupancyRecordValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered occupancy record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredOccupancyRecordStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered occupancy record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredOccupancyRecordStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of tiered occupancy record workflow.</summary>
public static class TieredOccupancyRecordWorkflow
{
    public static TieredOccupancyRecordManager NewTieredOccupancyRecordManager(ITieredOccupancyRecordStore store, IClock clock)
    {
        return new TieredOccupancyRecordManager(
            store,
            TieredOccupancyRecordRules.DefaultTieredOccupancyRecordValidator(),
            clock,
            new List<string>());
    }
}
