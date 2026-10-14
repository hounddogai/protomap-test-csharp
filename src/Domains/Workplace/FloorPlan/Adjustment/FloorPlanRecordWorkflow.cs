namespace Acme.Workforce.Workplace.FloorPlan.Adjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves floor plan records through their life cycle and keeps a journal of what it did.</summary>
public sealed record FloorPlanRecordService(IFloorPlanRecordStore Store, FloorPlanRecordValidator Validator, IClock Clock, List<string> Journal, FloorPlanRecordEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FloorPlanRecordStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FloorPlanRecordStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan record {code} not found");
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of floor plan record workflow.</summary>
public static class FloorPlanRecordWorkflow
{
    public static FloorPlanRecordService NewFloorPlanRecordService(IFloorPlanRecordStore store, IClock clock)
    {
        return new FloorPlanRecordService(
            store,
            FloorPlanRecordRules.DefaultFloorPlanRecordValidator(),
            clock,
            new List<string>(),
            FloorPlanRecordEngine.DefaultFloorPlanRecordEngine());
    }
}
