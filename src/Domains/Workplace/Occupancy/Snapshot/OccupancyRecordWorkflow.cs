namespace Acme.Workforce.Workplace.Occupancy.Snapshot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves occupancy records through their life cycle and keeps a journal of what it did.</summary>
public sealed record OccupancyRecordDesk(IOccupancyRecordStore Store, OccupancyRecordValidator Validator, IClock Clock, List<string> Journal, OccupancyRecordEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(OccupancyRecordStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(OccupancyRecordStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy record {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Audit(int cutoff)
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

/// <summary>Functions of occupancy record workflow.</summary>
public static class OccupancyRecordWorkflow
{
    public static OccupancyRecordDesk NewOccupancyRecordDesk(IOccupancyRecordStore store, IClock clock)
    {
        return new OccupancyRecordDesk(
            store,
            OccupancyRecordRules.DefaultOccupancyRecordValidator(),
            clock,
            new List<string>(),
            OccupancyRecordEngine.DefaultOccupancyRecordEngine());
    }
}
