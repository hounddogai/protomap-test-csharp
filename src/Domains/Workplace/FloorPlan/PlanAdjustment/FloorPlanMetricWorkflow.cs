namespace Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves floor plan metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record FloorPlanMetricService(IFloorPlanMetricStore Store, FloorPlanMetricValidator Validator, IClock Clock, List<string> Journal, FloorPlanMetricEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FloorPlanMetricStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FloorPlanMetricStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan metric {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of floor plan metric workflow.</summary>
public static class FloorPlanMetricWorkflow
{
    public static FloorPlanMetricService NewFloorPlanMetricService(IFloorPlanMetricStore store, IClock clock, Registry flags)
    {
        return new FloorPlanMetricService(
            store,
            FloorPlanMetricRules.DefaultFloorPlanMetricValidator(),
            clock,
            new List<string>(),
            FloorPlanMetricEngine.DefaultFloorPlanMetricEngine(flags));
    }
}
