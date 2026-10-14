namespace Acme.Workforce.Workplace.FloorPlan.PlanEntry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves rolling floor plan entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record RollingFloorPlanEntryManager(IRollingFloorPlanEntryStore Store, RollingFloorPlanEntryValidator Validator, IClock Clock, List<string> Journal, RollingFloorPlanEntryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling floor plan entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingFloorPlanEntryStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling floor plan entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingFloorPlanEntryStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling floor plan entry {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Audit(int cutoff)
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

/// <summary>Functions of rolling floor plan entry workflow.</summary>
public static class RollingFloorPlanEntryWorkflow
{
    public static RollingFloorPlanEntryManager NewRollingFloorPlanEntryManager(IRollingFloorPlanEntryStore store, IClock clock, Registry flags)
    {
        return new RollingFloorPlanEntryManager(
            store,
            RollingFloorPlanEntryRules.DefaultRollingFloorPlanEntryValidator(),
            clock,
            new List<string>(),
            RollingFloorPlanEntryEngine.DefaultRollingFloorPlanEntryEngine(flags));
    }
}
