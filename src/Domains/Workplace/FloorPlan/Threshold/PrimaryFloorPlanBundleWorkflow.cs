namespace Acme.Workforce.Workplace.FloorPlan.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves primary floor plan bundles through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryFloorPlanBundleCoordinator(IPrimaryFloorPlanBundleStore Store, PrimaryFloorPlanBundleValidator Validator, IClock Clock, List<string> Journal, PrimaryFloorPlanBundleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary floor plan bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryFloorPlanBundleStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary floor plan bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryFloorPlanBundleStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary floor plan bundle {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of primary floor plan bundle workflow.</summary>
public static class PrimaryFloorPlanBundleWorkflow
{
    public static PrimaryFloorPlanBundleCoordinator NewPrimaryFloorPlanBundleCoordinator(IPrimaryFloorPlanBundleStore store, IClock clock, Registry flags)
    {
        return new PrimaryFloorPlanBundleCoordinator(
            store,
            PrimaryFloorPlanBundleRules.DefaultPrimaryFloorPlanBundleValidator(),
            clock,
            new List<string>(),
            PrimaryFloorPlanBundleEngine.DefaultPrimaryFloorPlanBundleEngine(flags));
    }
}
