namespace Acme.Workforce.Workplace.FloorPlan.Cycle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive floor plan metrics through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record AdaptiveFloorPlanMetricManager(IAdaptiveFloorPlanMetricStore Store, AdaptiveFloorPlanMetricValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive floor plan metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveFloorPlanMetricStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive floor plan metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveFloorPlanMetricStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of adaptive floor plan metric workflow.</summary>
public static class AdaptiveFloorPlanMetricWorkflow
{
    public static AdaptiveFloorPlanMetricManager NewAdaptiveFloorPlanMetricManager(IAdaptiveFloorPlanMetricStore store, IClock clock)
    {
        return new AdaptiveFloorPlanMetricManager(
            store,
            AdaptiveFloorPlanMetricRules.DefaultAdaptiveFloorPlanMetricValidator(),
            clock,
            new List<string>());
    }
}
