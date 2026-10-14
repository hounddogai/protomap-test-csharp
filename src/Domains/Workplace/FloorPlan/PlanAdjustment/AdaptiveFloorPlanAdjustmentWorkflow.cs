namespace Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves adaptive floor plan adjustments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record AdaptiveFloorPlanAdjustmentCoordinator(IAdaptiveFloorPlanAdjustmentStore Store, AdaptiveFloorPlanAdjustmentValidator Validator, IClock Clock, List<string> Journal, AdaptiveFloorPlanAdjustmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive floor plan adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveFloorPlanAdjustmentStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive floor plan adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveFloorPlanAdjustmentStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive floor plan adjustment {code} not found");
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

/// <summary>Functions of adaptive floor plan adjustment workflow.</summary>
public static class AdaptiveFloorPlanAdjustmentWorkflow
{
    public static AdaptiveFloorPlanAdjustmentCoordinator NewAdaptiveFloorPlanAdjustmentCoordinator(IAdaptiveFloorPlanAdjustmentStore store, IClock clock, Registry flags)
    {
        return new AdaptiveFloorPlanAdjustmentCoordinator(
            store,
            AdaptiveFloorPlanAdjustmentRules.DefaultAdaptiveFloorPlanAdjustmentValidator(),
            clock,
            new List<string>(),
            AdaptiveFloorPlanAdjustmentEngine.DefaultAdaptiveFloorPlanAdjustmentEngine(flags));
    }
}
