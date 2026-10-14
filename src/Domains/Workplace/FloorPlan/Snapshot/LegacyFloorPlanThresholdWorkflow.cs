namespace Acme.Workforce.Workplace.FloorPlan.Snapshot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves legacy floor plan thresholds through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LegacyFloorPlanThresholdService(ILegacyFloorPlanThresholdStore Store, LegacyFloorPlanThresholdValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy floor plan threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyFloorPlanThresholdStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy floor plan threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyFloorPlanThresholdStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of legacy floor plan threshold workflow.</summary>
public static class LegacyFloorPlanThresholdWorkflow
{
    public static LegacyFloorPlanThresholdService NewLegacyFloorPlanThresholdService(ILegacyFloorPlanThresholdStore store, IClock clock)
    {
        return new LegacyFloorPlanThresholdService(
            store,
            LegacyFloorPlanThresholdRules.DefaultLegacyFloorPlanThresholdValidator(),
            clock,
            new List<string>());
    }
}
