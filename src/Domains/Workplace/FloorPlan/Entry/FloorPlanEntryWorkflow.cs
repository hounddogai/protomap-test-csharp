namespace Acme.Workforce.Workplace.FloorPlan.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves floor plan entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record FloorPlanEntryDesk(IFloorPlanEntryStore Store, FloorPlanEntryValidator Validator, IClock Clock, List<string> Journal, FloorPlanEntryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FloorPlanEntryStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FloorPlanEntryStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan entry {code} not found");
        }
        var result = Engine.Evaluate(item);
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of floor plan entry workflow.</summary>
public static class FloorPlanEntryWorkflow
{
    public static FloorPlanEntryDesk NewFloorPlanEntryDesk(IFloorPlanEntryStore store, IClock clock, Registry flags)
    {
        return new FloorPlanEntryDesk(
            store,
            FloorPlanEntryRules.DefaultFloorPlanEntryValidator(),
            clock,
            new List<string>(),
            FloorPlanEntryEngine.DefaultFloorPlanEntryEngine(flags));
    }
}
