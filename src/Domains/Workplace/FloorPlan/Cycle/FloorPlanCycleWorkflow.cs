namespace Acme.Workforce.Workplace.FloorPlan.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves floor plan cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record FloorPlanCycleService(IFloorPlanCycleStore Store, FloorPlanCycleValidator Validator, IClock Clock, List<string> Journal, FloorPlanCycleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FloorPlanCycleStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FloorPlanCycleStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan cycle {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of floor plan cycle workflow.</summary>
public static class FloorPlanCycleWorkflow
{
    public static FloorPlanCycleService NewFloorPlanCycleService(IFloorPlanCycleStore store, IClock clock, Registry flags)
    {
        return new FloorPlanCycleService(
            store,
            FloorPlanCycleRules.DefaultFloorPlanCycleValidator(),
            clock,
            new List<string>(),
            FloorPlanCycleEngine.DefaultFloorPlanCycleEngine(flags));
    }
}
