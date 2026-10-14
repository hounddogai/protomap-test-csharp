namespace Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves floor plan items through their life cycle and keeps a journal of what it did.</summary>
public sealed record FloorPlanItemCoordinator(IFloorPlanItemStore Store, FloorPlanItemValidator Validator, IClock Clock, List<string> Journal, FloorPlanItemEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan item {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FloorPlanItemStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan item {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FloorPlanItemStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan item {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of floor plan item workflow.</summary>
public static class FloorPlanItemWorkflow
{
    public static FloorPlanItemCoordinator NewFloorPlanItemCoordinator(IFloorPlanItemStore store, IClock clock, Registry flags)
    {
        return new FloorPlanItemCoordinator(
            store,
            FloorPlanItemRules.DefaultFloorPlanItemValidator(),
            clock,
            new List<string>(),
            FloorPlanItemEngine.DefaultFloorPlanItemEngine(flags));
    }
}
