namespace Acme.Workforce.Workplace.FloorPlan.Adjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves floor plan adjustments through their life cycle and keeps a journal of what it did.</summary>
public sealed record FloorPlanAdjustmentDesk(IFloorPlanAdjustmentStore Store, FloorPlanAdjustmentValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FloorPlanAdjustmentStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FloorPlanAdjustmentStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public int ReviewStale(int cutoff)
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

/// <summary>Functions of floor plan adjustment workflow.</summary>
public static class FloorPlanAdjustmentWorkflow
{
    public static FloorPlanAdjustmentDesk NewFloorPlanAdjustmentDesk(IFloorPlanAdjustmentStore store, IClock clock)
    {
        return new FloorPlanAdjustmentDesk(
            store,
            FloorPlanAdjustmentRules.DefaultFloorPlanAdjustmentValidator(),
            clock,
            new List<string>());
    }
}
