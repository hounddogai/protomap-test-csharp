namespace Acme.Workforce.Workplace.FloorPlan.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves floor plan templates through their life cycle and keeps a journal of what it did.</summary>
public sealed record FloorPlanTemplateCoordinator(IFloorPlanTemplateStore Store, FloorPlanTemplateValidator Validator, IClock Clock, List<string> Journal, FloorPlanTemplateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FloorPlanTemplateStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FloorPlanTemplateStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan template {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of floor plan template workflow.</summary>
public static class FloorPlanTemplateWorkflow
{
    public static FloorPlanTemplateCoordinator NewFloorPlanTemplateCoordinator(IFloorPlanTemplateStore store, IClock clock, Registry flags)
    {
        return new FloorPlanTemplateCoordinator(
            store,
            FloorPlanTemplateRules.DefaultFloorPlanTemplateValidator(),
            clock,
            new List<string>(),
            FloorPlanTemplateEngine.DefaultFloorPlanTemplateEngine(flags));
    }
}
